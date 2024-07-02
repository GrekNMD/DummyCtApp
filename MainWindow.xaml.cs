using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;

using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DummyCtApp
{
    public class RecevedImage
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        bool drag = false;
        Point startPoint;
        Rectangle rectangle;
        Image mainImage;
        string croppedImagesPath = @"C:\Temp\CroppedImages\";
        static int cbImageIndex = 1;

        private ObservableCollection<BitmapImage> _croppedImages = new ObservableCollection<BitmapImage>();
        public ObservableCollection<BitmapImage> CroppedImages
        {
            get { return _croppedImages; }
            set
            {
                _croppedImages = value;
                OnPropertyChanged("CroppedImages");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            // Fire the PropertyChanged event when a property is changed
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

        }

        private void rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // start dragging
            drag = true;
            // save start point of dragging
            startPoint = Mouse.GetPosition(canvas1);
        }

        private void rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            
            // if dragging, then adjust rectangle position based on mouse movement
            if (drag)
            {
                double d = canvas1.Height;
                Rectangle draggedRectangle = sender as Rectangle;
                Point newPoint = Mouse.GetPosition(canvas1);
                //double left = Canvas.GetLeft(draggedRectangle);
                double top = Canvas.GetTop(draggedRectangle);
                //Canvas.SetLeft(draggedRectangle, left + (newPoint.X - startPoint.X));
                Canvas.SetTop(draggedRectangle, top + (newPoint.Y - startPoint.Y));

                startPoint = newPoint;
            }
        }

        private void rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // stop dragging
            drag = false;
        }
        private void Create_PlanBox(object sender, RoutedEventArgs e)
        {
            if (canvas1.Children.Contains(rectangle))
            {
                return;
            }

            rectangle = new Rectangle()
            {
                Width = 204,
                Height = 20,
                Fill = Brushes.LightBlue,
                Opacity = 0.5,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
            };
            rectangle.MouseDown += rectangle_MouseDown;
            rectangle.MouseMove += rectangle_MouseMove;
            rectangle.MouseUp += rectangle_MouseUp;

            
            canvas1.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, 0); // Set the initial left position
            Canvas.SetTop(rectangle, 50); // Set the initial top position

        }

        private Stream ConvertToStream(BitmapSource bitmapImage)
        {
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new JpegBitmapEncoder(); // Change the encoder based on your needs (e.g., JpegBitmapEncoder)
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(stream);
            return stream;
        }

        private string SaveCroppedImage(BitmapSource bitmapSource, string path)
        {
            // Save the cropped image to a file
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();  
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
            }
            return path;
        }
        private void Manual_Scan(object sender, RoutedEventArgs e)
        {
            
            int y = (int)Canvas.GetTop(rectangle);
            int x = (int)Canvas.GetLeft(rectangle);
            int w = (int)rectangle.Width;
            int h = (int)rectangle.Height;

            //Create a CroppedBitmap based off of a xaml defined resource.
            CroppedBitmap cb = new CroppedBitmap(
               (BitmapSource)this.Resources["masterImage"],
                    new Int32Rect(x, y, w, h)); //select region rect
            //secondImage.Source = cb;                 //set image source to cropped
            string cbImageName = $"croppedImage_{cbImageIndex}.jpg";
            string path = SaveCroppedImage(cb, croppedImagesPath + cbImageName);
            cbImageIndex++;

            BitmapImage bitmapImage = new BitmapImage(new Uri(path));

            // Add the new cropped image to the ObservableCollection
            CroppedImages.Add(bitmapImage);

            // Notify the UI of the changes to the ObservableCollection
            OnPropertyChanged("CroppedImages");


            //// Set the list of images as the ItemsSource for the ListView
            _imagelistView.ItemsSource = CroppedImages;

        }
    }
}