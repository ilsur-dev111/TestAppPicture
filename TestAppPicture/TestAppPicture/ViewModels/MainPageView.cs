using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TestAppPicture.Models;
using TestAppPicture.Services;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace TestAppPicture.Views
{
	public class MainPageView : INotifyPropertyChanged
    {
        
        public MainPageView()
        {
            IDevice device = DependencyService.Get<IDevice>();
            // получаем идентификатор
            ID = device.GetIdentifier();

            #region инициализация команд
            StartStopRun = new Command(() =>
            {
                if (RequestTask.IsCompleted)
                    Start();
                else
                    Stop();
            });
            #endregion

            // запускаем задачу
            Start();
        }

        public Command StartStopRun { get; set; }

        #region идентификатор
        string IDPrivate { get; set; } = "";
        public string ID
        {
            get { return IDPrivate; }
            set
            {
                IDPrivate = value;
                OnPropertyChanged("Source");
            }
        }
        #endregion

        #region Модель в которую записываем ответ
        ModelRequest ModelPrivate { get; set; }
        ModelRequest Model
        {
            get { return ModelPrivate; }
            set
            {
                ModelPrivate = value;
                if (value != null)
                {
                    OnPropertyChanged("Text");
                    OnPropertyChanged("Source");
                }
            }
        }
        #endregion

        #region текст кнопки запуск/стоп для MVVM
        public string TextButton
        {
            get
            {
                return RequestTask is null ? "Запустить" : RequestTask.IsCompleted ? "Запустить" :
                    CancellTolen.IsCancellationRequested ? "Останавливается..." : "Остановить";
            }
        }
        #endregion

        #region статус работы
        // текст для MVVM
        string StatusPrivate { get; set; } = "";
        public string Status
        {
            get { return StatusPrivate; }
            set
            {
                StatusPrivate = value;
                OnPropertyChanged("Status");
            }
        }

        // цвет текста для MVVM
        Color StatusColorPrivate { get; set; } = Color.IndianRed;
        public Color StatusColor
        {
            get { return StatusColorPrivate; }
            set
            {
                StatusColorPrivate = value;
                OnPropertyChanged("StatusColor");
            }
        }
        #endregion

        #region текст полученный с запроса для MVVM
        public string Text
        {
            get { return Model is null ? "" : Model.Text; }
        }
        #endregion

        #region полученное изображение для MVVM
        public ImageSource Source
        {
            get
            {
                return ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(Model.Picture)));
            }
        }
        #endregion

        Task RequestTask;
        CancellationTokenSource CancellTolen;
        #region Продолжить/начать получать изображения
        public void Start()
        {
            if (RequestTask != null && !RequestTask.IsCompleted)
                return; // что бы повторно не запускать операцию

            CancellTolen = new CancellationTokenSource();
            CancellationToken ct = CancellTolen.Token;
            #region сама задача
            RequestTask = Task.Run(async () =>
            {
                ct.ThrowIfCancellationRequested();
                bool ShowNotConnectionMessage = false; // показывали ли сообщение о отсутствии подключения 
                while (true)
                {
                    #region если нет подключения
                    if (!WebRequestProtocol.CheckInternetConnection())
                    {
                        if (!ShowNotConnectionMessage)
                        {
                            Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Нет подключения к сети интернет, проверьте подключение к сети интернет", "Хорошо"));
                            ShowNotConnectionMessage = true;
                            Status = "Нет подключения";
                            StatusColor = Color.IndianRed;
                        }
                    }
                    #endregion
                    #region если есть подключение
                    else
                    {
                        ShowNotConnectionMessage = false;
                        try
                        {
                            // выполняем запрос
                            string Result = await WebRequestProtocol.WebRequestProtocolJSON_GETAsync("https://mobile.netix.ru/MobileWebResource.asmx/GetTextAndPicture",
                                new List<ParammRequest_GET>() { new ParammRequest_GET { Key = "id", Paramm = ID } });

                            // дессериализуем данные
                            if(Result!=null) 
                                Model = JsonSerializer.Deserialize<ModelRequest>(Result);

                            if (Status != "Выполняется")
                            {
                                Status = "Выполняется";
                                StatusColor = Color.Green;
                            }

                        }
                        catch(JsonException ex)
                        {
                            Status = "Ошибка JSON";
                            StatusColor = Color.IndianRed;
                        }
                        catch (Exception ex)
                        {
                            Status = "Неизвестная ошибка";
                            StatusColor = Color.IndianRed;
                        }
                    }
                    #endregion

                    if (ct.IsCancellationRequested)
                    {
                        Status = "Остановлен";
                        StatusColor = Color.DarkGray;
                        OnPropertyChanged("TextButton");
                        ct.ThrowIfCancellationRequested();
                    }

                    // для успокоения внутреннего хардварщика
                    await Task.Delay(500);
                }
            }, CancellTolen.Token);
            #endregion



            OnPropertyChanged("TextButton");
        }
        #endregion

        #region остановиться получать изображения
        public void Stop()
        {
            CancellTolen.Cancel();
            OnPropertyChanged("TextButton");
        }
        #endregion

        #region Зона уведомления о изменении привязанных значений для MVVM
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }


    
}

