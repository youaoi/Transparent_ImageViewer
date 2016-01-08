using System;
using System.Collections.Generic;
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
using System.IO;

using System.Drawing;
using System.Drawing.Imaging;

using Forms = System.Windows.Forms;
using Rectangle = System.Drawing.Rectangle;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Image = System.Drawing.Image;

namespace MediaViewer
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    public const string FILTER = "Image File (*.jpg,*.png,*.gif)|*.jpg;*.png;*.gif";
    private BitmapImage bi;
    public MainWindow()
    {
      InitializeComponent();

      this.Loaded += main_Loaded;
      this.Drop += main_Drop;
      this.SizeChanged += main_SizeChanged;
      this.Closing += main_Unload;
      
      this.sldOpacity.ValueChanged += sldOpacity_ValueChanged;
      this.btnTitle.Click += lblTitle_DoubleClick;

      this.sizeX.Visibility = System.Windows.Visibility.Collapsed;
      this.sizeY.Visibility = System.Windows.Visibility.Collapsed;

      var setting = new ImageViewer.Properties.Settings();
      this.Left = setting.Left;
      this.Top = setting.Top;
      this.chkTopMost.IsChecked = setting.Topmost;
      this.sldOpacity.Value = setting.Opacity;
    }

    private void main_Loaded(object sender, RoutedEventArgs e)
    {
      if (App.ArgString != null)
      {
        loadImage(App.ArgString);
      }
    }

    private void lblTitle_DoubleClick(object sender, RoutedEventArgs e)
    {
      var ofd = new Forms.OpenFileDialog();
      ofd.Multiselect = false;
      ofd.ShowReadOnly = false;
      ofd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
      ofd.Filter = FILTER;

      var dr = ofd.ShowDialog();
      if (dr != Forms.DialogResult.OK) return;

      loadImage(ofd.FileNames[0]);
    }

    private void main_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this.WindowState != System.Windows.WindowState.Maximized)
        this.btnMaximize.Content = "1";
      else
        this.btnMaximize.Content = "2";

      var x = this.Width - this.gridBody.Margin.Left + this.gridBody.Margin.Right;
      var y = this.Height - this.gridBody.Margin.Top + this.gridBody.Margin.Bottom;
      sizeX.Content = x.ToString();
      sizeY.Content = y.ToString();
    }

    private void main_Drop(object sender, DragEventArgs e)
    {
      var myDataObject = e.Data as IDataObject;
      var dropFiles = myDataObject.GetData(DataFormats.FileDrop) as string[];
      loadImage(dropFiles[0]);
    }

    private void loadImage(string uri)
    {
      this.bi = new BitmapImage();
      this.bi.BeginInit();
      this.bi.UriSource = new Uri(uri);
      this.bi.EndInit();

      this.MediaLoaded(null, null);

      this.img.Source = bi;
    }

    private void MediaLoaded(object sender, EventArgs e)
    {
      this.changeImageSize(this.bi.Width, this.bi.Height);
    }

    private void changeImageSize(double x, double y)
    {
      if (x > 0)
      {
        var w = x + this.gridBody.Margin.Left + this.gridBody.Margin.Right;
        this.Width = w;
      }
      if (y > 0)
      {
        var h = y + this.gridBody.Margin.Top + this.gridBody.Margin.Bottom;
        this.Height = h;
      }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      this.DragMove();
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void btnMaximize_Click(object sender, RoutedEventArgs e)
    {
      if (this.WindowState != System.Windows.WindowState.Maximized)
        this.WindowState = System.Windows.WindowState.Maximized;
      else
        this.WindowState = System.Windows.WindowState.Normal;
    }

    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
      this.WindowState = System.Windows.WindowState.Minimized;
    }

    private void sldOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      this.img.Opacity = sldOpacity.Value / 100.0;
    }

    private void chkTopMost_Checked(object sender, RoutedEventArgs e)
    {
      var o = sender as CheckBox;
      this.Topmost = o.IsChecked.Value;
    }

    private void chkTopMost_Unchecked(object sender, RoutedEventArgs e)
    {
      var o = sender as CheckBox;
      this.Topmost = o.IsChecked.Value;
    }

    private void btnSize_Click(object sender, RoutedEventArgs e)
    {
      var b = (sizeX.Visibility != System.Windows.Visibility.Visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

      this.sizeX.Visibility = b;
      this.sizeY.Visibility = b;
      /*
      var bm = ToBitmap(img.Source as BitmapImage);
      if (bm != null)
      {
        //ChangeToNegativeImage(bm);
        img.Source = ToBitmapImage(bm);
      }*/
    }

    /*
    public static Bitmap ToBitmap(BitmapImage bitmapImage)
    {
      if (bitmapImage == null) return null;

      using (MemoryStream outStream = new MemoryStream())
      {
        BitmapEncoder enc = new BmpBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(bitmapImage));
        enc.Save(outStream);
        Bitmap bitmap = new Bitmap(outStream);

        return new Bitmap(bitmap);
      }
    }
    public static BitmapImage ToBitmapImage(Bitmap bitmap)
    {
      if (bitmap == null) return null;

      using (var memory = new MemoryStream())
      {
        bitmap.Save(memory, ImageFormat.Png);
        memory.Position = 0;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        return bitmapImage;
      }
    }

    public static void ChangeToNegativeImage(Bitmap img)
    {
      //1ピクセルあたりのバイト数を取得する
      PixelFormat pixelFormat = img.PixelFormat;
      int pixelSize = Image.GetPixelFormatSize(pixelFormat) / 8;
      if (pixelSize < 3 || 4 < pixelSize)
      {
        throw new ArgumentException(
            "1ピクセルあたり24または32ビットの形式のイメージのみ有効です。",
            "img");
      }

      //Bitmapをロックする
      BitmapData bmpDate = img.LockBits(
          new Rectangle(0, 0, img.Width, img.Height),
          ImageLockMode.ReadWrite,
          pixelFormat);

      if (bmpDate.Stride < 0)
      {
        img.UnlockBits(bmpDate);
        throw new ArgumentException(
            "ボトムアップ形式のイメージには対応していません。",
            "img");
      }

      //ピクセルデータをバイト型配列で取得する
      IntPtr ptr = bmpDate.Scan0;
      byte[] pixels = new byte[bmpDate.Stride * img.Height];
      System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);

      //すべてのピクセルの色を反転させる
      for (int y = 0; y < bmpDate.Height; y++)
      {
        for (int x = 0; x < bmpDate.Width; x++)
        {
          //ピクセルデータでのピクセル(x,y)の開始位置を計算する
          int pos = y * bmpDate.Stride + x * pixelSize;
          //青、緑、赤の色を変更する
          pixels[pos] = (byte)(255 - pixels[pos]);
          pixels[pos + 1] = (byte)(255 - pixels[pos + 1]);
          pixels[pos + 2] = (byte)(255 - pixels[pos + 2]);
        }
      }

      //ピクセルデータを元に戻す
      System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);

      //ロックを解除する
      img.UnlockBits(bmpDate);
    }*/

    private void main_Unload(object sender, System.ComponentModel.CancelEventArgs e)
    {
      var setting = new ImageViewer.Properties.Settings();
      setting.Left = (int)this.Left;
      setting.Top = (int)this.Top;
      setting.Topmost = this.chkTopMost.IsChecked.Value;
      setting.Opacity = (ushort)this.sldOpacity.Value;
      setting.Save();
    }

  }
}
