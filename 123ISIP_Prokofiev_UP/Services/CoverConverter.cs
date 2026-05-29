using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using _123ISIP_Prokofiev_UP.Models;

namespace _123ISIP_Prokofiev_UP.Services
{

    public static class Covers
    {
        private static readonly string Folder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Covers");

        public static BitmapImage Load(int bookId, string coverPath)
        {
            string file = Path.Combine(Folder, bookId + ".png");
            if (!File.Exists(file))
                file = (!string.IsNullOrWhiteSpace(coverPath) && File.Exists(coverPath)) ? coverPath : null;
            if (file == null) return null;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(file, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        private static (int id, string path)? Extract(object value)
        {
            if (value is Book b) return (b.Id, b.CoverPath);
            if (value is ReadingListItem r) return (r.BookId, r.CoverPath);
            return null;
        }

        public static BitmapImage From(object value)
        {
            var info = Extract(value);
            return info.HasValue ? Load(info.Value.id, info.Value.path) : null;
        }
    }

    public class CoverConverter : IValueConverter
    {
        private static readonly SolidColorBrush Placeholder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2A32"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var img = Covers.From(value);
            string mode = parameter as string ?? "brush";

            if (mode == "icon")
                return img == null ? Visibility.Visible : Visibility.Collapsed;

            if (img == null) return Placeholder;
            return new ImageBrush(img) { Stretch = Stretch.UniformToFill };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
