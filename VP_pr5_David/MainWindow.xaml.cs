using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;

//Написать программу, позволяющую разместить на графической сцене произвольное число объектов следующих двух типов:
//1.Пентаграмма тёмно - лососёвого цвета
//2.Вертолёт, нарисованный программно
//Должно быть предусмотрено перемещение добавляемых объектов путём перетаскивания их левой кнопкой мыши.
//С помощью обработки события нажатия клавиши мыши должно быть реализовано
//следующее изменение объектов при нажатии на них правой кнопкой мыши:
//1.Первый объект начинает вращаться по часовой стрелке
//2.Второй объект взрывается с анимацией взрыва 


namespace VP_pr5_David
{
    public class CustomControl : Control
    {
        protected static string imgDirectory = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\img\\";
        public CustomControl() { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
    }
    public class Pentagram : CustomControl
    {
        public static string imgLink = imgDirectory + "\\pentagram.png";
        public Image pent_image;
        //public bool doRotation = false;
        public double rDegree = 0;
        public Pentagram()
        {

            
        }

        public Image GetImage()
        {
            pent_image = new Image();
            pent_image.Width = 90;
            pent_image.Height = 90;
            pent_image.Source = new BitmapImage(new Uri("img/pentagram.png", UriKind.Relative));
            return pent_image;
        }
    }
    public partial class MainWindow : Window
    {
        public double singleRotatationDegree = 15;
        private UIElement draggedItem;
        private Point itemRelativePosition;
        Random rnd = new Random();
        List<Pentagram> pList = new List<Pentagram>();
        List<Image> pImages = new List<Image>();
        //Timer rotationTimer;
        public MainWindow()
        {
            InitializeComponent();

            PreviewKeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };

            //rotationTimer = new Timer(100);
            //rotationTimer.Elapsed += MakeRotation;
            //rotationTimer.Enabled = true;
        }


        //Обработчик "захвата" графического объекта
        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Данный объект назначается текущим передвигаемым
            this.draggedItem = (UIElement)sender;
            //Сохраняется его текущая позиция
            itemRelativePosition = e.GetPosition(this.draggedItem);
            //Пометка что событие обработано и больше его обрабатывать не требуется
            e.Handled = true;
        }
        //Обработчик передвижения графического объекта
        private void MainCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //Если передвигаемого объекта нет - закончить обработку события
            if (this.draggedItem == null)
                return;
            //Иначе - вычислить новые координаты относительно положения Canvas
            var newPos = e.GetPosition(MainCanvas) - itemRelativePosition;
            //Установить требуемое смещение
            Canvas.SetTop(this.draggedItem, newPos.Y);
            Canvas.SetLeft(this.draggedItem, newPos.X);
            //Canvas захватывает мышь во избежание случайной реакции со стороны других элементов
            MainCanvas.CaptureMouse();
            //Пометка что событие обработано и больше его обрабатывать не требуется
            e.Handled = true;
        }
        //Обработчик "отпускания" графического объекта
        private void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Если объект в данный момент переносился
            if (this.draggedItem != null)
            {
                //Объект отпускается
                this.draggedItem = null;
                //Canvas "отпускает" мышь
                MainCanvas.ReleaseMouseCapture();
                //Пометка что событие обработано и больше его обрабатывать не требуется
                e.Handled = true;
            }
        }
        private void Add_Image(Image image)
        {
            int x = rnd.Next(90, (int)MainCanvas.ActualWidth - 90), y = rnd.Next(90, (int)MainCanvas.ActualHeight - 90);
            MainCanvas.Children.Add(image);
            Canvas.SetTop(image, y);
            Canvas.SetLeft(image, x);
        }
        private void AddHeli(object sender, RoutedEventArgs e)
        {
            Helicopter heli = new Helicopter();
            heli.MouseRightButtonDown += BlowUp;
            heli.MouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            heli.Cursor = Cursors.Hand;
            int x = rnd.Next(90, (int)MainCanvas.ActualWidth - 90), y = rnd.Next(90, (int)MainCanvas.ActualHeight - 90);
            Canvas.SetTop(heli, y);
            Canvas.SetLeft(heli, x);
            MainCanvas.Children.Add(heli);
        }

        private void AddPent(object sender, RoutedEventArgs e)
        {
            Pentagram pent = new Pentagram();
            pList.Add(pent);
            Image pent_image = pent.GetImage();
            pent_image.Name = "pentagram" + pList.Count.ToString();
            pent_image.Cursor = Cursors.Hand;
            pent_image.PreviewMouseRightButtonDown += Rotate;
            pent_image.PreviewMouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            Panel.SetZIndex(pent_image, 2);
            Add_Image(pent_image);
            pImages.Add(pent_image);
        }

        private void Rotate(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;
            int num;
            int.TryParse(image.Name[9].ToString(), out num);
            Pentagram pent = pList[num - 1];
            //pent.doRotation = !pent.doRotation;
            pent.rDegree += singleRotatationDegree;
            if (pent.rDegree >= 360)
                pent.rDegree = 0;
            RotateTransform rotateTransform = new RotateTransform(pent.rDegree);
            image.RenderTransform = rotateTransform;
        }
        //public void MakeRotation(Object source, ElapsedEventArgs e)
        //{
        //    int count = 0;
        //    foreach (var pent in pList)
        //    {
        //        if (pent.doRotation)
        //        {
        //            Image image = pImages[count++];
        //            pent.rDegree += singleRotatationDegree;
        //            if (pent.rDegree >= 360)
        //                pent.rDegree = 0;
        //            RotateTransform rotateTransform = new RotateTransform(pent.rDegree);
        //            image.RenderTransform = rotateTransform;
        //        }
        //    }

        //}
        private void BlowUp(object sender, MouseButtonEventArgs e)
        {
            Helicopter heli = (Helicopter)sender;
            double x = Canvas.GetLeft(heli);
            double y = Canvas.GetTop(heli);
            heli.IsEnabled = false;
            MainCanvas.Children.Remove(heli);

            BlewUpHeli bheli = new BlewUpHeli();
            bheli.MouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            Canvas.SetTop(bheli, y);
            Canvas.SetLeft(bheli, x);
            MainCanvas.Children.Add(bheli);

           
        }
    }
}
