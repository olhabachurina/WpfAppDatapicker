using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.ComponentModel;
namespace WpfAppDatapicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Booking
    {
        public string Name { get; set; }
        public string RoomNumber { get; set; }
        public int Duration { get; set; }
        public DateTime Date { get; set; }
        public DateTime CheckInDate { get; internal set; }
        public DateTime CheckOutDate { get; internal set; }
        public int StayDuration { get; internal set; }
    }


    public partial class MainWindow : Window
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<Booking> allBookings = new List<Booking>(); 
        private ObservableCollection<DateTime> availableDates;
        private DatePicker datePicker;
       
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<DateTime> AvailableDates
        {
            get { return availableDates; }
            set
            {
                if (value != availableDates)
                {
                    availableDates = value;
                    OnPropertyChanged(nameof(AvailableDates));
                }
            }
        }

        private ObservableCollection<Booking> bookings;
        public ObservableCollection<Booking> Bookings
        {
            get { return bookings; }
            set
            {
                if (value != bookings)
                {
                    bookings = value;
                    OnPropertyChanged(nameof(Bookings));
                }
            }
        }

        private DateTime selectedDate;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                if (value != selectedDate)
                {
                    selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            availableDates = new ObservableCollection<DateTime>();
            Bookings = new ObservableCollection<Booking>(); 

            datePicker = new DatePicker();
            datePicker.DisplayDateStart = DateTime.Today;
            datePicker.SelectedDate = DateTime.Today;
            datePicker.BlackoutDates.AddDatesInPast();

            availableDatesListBox.ItemsSource = AvailableDates;
            bookingsListBox.ItemsSource = Bookings;

            LoadAvailableDates();
        }
       
        private void LoadAvailableDates()
        {
            
            availableDates.Clear();

            foreach (DateTime date in Enumerable.Range(0, 10).Select(i => DateTime.Today.AddDays(i)))
            {
                bool isDateAvailable = !bookings.Any(booking =>
                    date >= booking.Date && date <= booking.Date.AddDays(booking.Duration - 1));

                if (isDateAvailable)
                {
                    availableDates.Add(date);
                }
            }
        }
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
            {
                SelectedDate = datePicker.SelectedDate.Value; 
                LoadBookings(SelectedDate);

                datePicker.BlackoutDates.Clear();
                foreach (Booking booking in Bookings)
                {
                    datePicker.BlackoutDates.Add(new CalendarDateRange(booking.CheckInDate, booking.CheckOutDate));
                }
            }
        }
    

            private void LoadBookings(DateTime selectedDate)
        {
           
            var selectedBookings = allBookings.Where(booking => booking.CheckInDate == selectedDate).ToList();
            Bookings.Clear();
            foreach (var booking in selectedBookings)
            {
                Bookings.Add(booking);
            }
        }
        private void CheckInDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDuration();
        }

        private void CheckOutDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDuration();
        }

        private void UpdateDuration()
        {
            if (checkInDatePicker.SelectedDate.HasValue && checkOutDatePicker.SelectedDate.HasValue)
            {
                TimeSpan duration = checkOutDatePicker.SelectedDate.Value - checkInDatePicker.SelectedDate.Value;
                durationTextBox.Text = duration.Days.ToString();
            }
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkInDatePicker.SelectedDate.HasValue &&
              checkOutDatePicker.SelectedDate.HasValue &&
              !string.IsNullOrEmpty(nameTextBox.Text) &&
              !string.IsNullOrEmpty(roomNumberTextBox.Text))
            {
                DateTime checkInDate = checkInDatePicker.SelectedDate.Value;
                DateTime checkOutDate = checkOutDatePicker.SelectedDate.Value;
                int duration = int.Parse(durationTextBox.Text);

                Booking newBooking = new Booking
                {
                    Name = nameTextBox.Text,
                    RoomNumber = roomNumberTextBox.Text,
                    Duration = duration,
                    Date = checkInDate
                };

                
                allBookings.Add(newBooking);

               
                LoadAvailableDates();
                LoadBookings(checkInDate);

                
                datePicker.BlackoutDates.Clear();
                foreach (Booking booking in Bookings)
                {
                    datePicker.BlackoutDates.Add(new CalendarDateRange(booking.Date, booking.Date.AddDays(booking.Duration - 1)));
                }

                nameTextBox.Clear();
                roomNumberTextBox.Clear();
                durationTextBox.Clear();
                checkInDatePicker.SelectedDate = null;
                checkOutDatePicker.SelectedDate = null;

                UpdateBookingsListBox();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные данные для бронирования.");
            }
        }

        private void UpdateBookingsListBox()
        {
            
            Bookings.Clear();

          
            var selectedBookings = allBookings.Where(booking => booking.Date == SelectedDate).ToList();


          
            foreach (var booking in selectedBookings)
            {
                Bookings.Add(booking);
            }

           
            OnPropertyChanged(nameof(Bookings));
        }
    
    

        private void UpdateBookingsListBox(DateTime checkInDate)
        {

           
            Bookings.Clear();

            
            var selectedBookings = Bookings.Where(booking => booking.Date == checkInDate).ToList();

           
            foreach (var booking in selectedBookings)
            {
                Bookings.Add(booking);
            }
        }
        private void checkInDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
            {
                DateTime checkInDate = datePicker.SelectedDate.Value;
               
                UpdateBookingsListBox(checkInDate);
            }
        }
        private List<Booking> GetBookingsForDate(DateTime checkInDate)
        {
            return allBookings.Where(booking => booking.Date == checkInDate).ToList();
        }

        
    }
}


