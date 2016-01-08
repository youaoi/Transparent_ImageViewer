using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer
{
  /// <summary>
  /// App.xaml の相互作用ロジック
  /// </summary>
  public partial class App : Application
  {
    public static string ArgString = null;
    private void Application_Startup(object sender, StartupEventArgs e)
    {
      foreach (string arg in e.Args)
      {
        ArgString = arg;
        break;
      }
    }
  }
}
