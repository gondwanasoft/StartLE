using CommunityToolkit.Mvvm.Input;
using H.Hooks;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
//using Microsoft.Windows.Management.Deployment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
//using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StartLE
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        //private Window? _window;
        private TaskbarIcon? _startTrayIcon;
        private MenuFlyout? _leftClickMenu;
        private MenuFlyout? _rightClickMenu;
        private Microsoft.UI.Dispatching.DispatcherQueue _uiQueue;
        //public ICommand TrayLeftClick { get; }
        //public ICommand TrayRightClick { get; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            /*this.UnhandledException += (sender, e) =>
            {
                var message = e.Message;    // breakpoint here to inspect
            };*/

            //var manager = new Microsoft.Windows.Management.Deployment.PackageDeploymentManager();
            //var result = manager.IsPackageRegistrationAllowed();

            _uiQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            //AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
            //AppNotificationManager.Default.Register();

            //_window = new MainWindow();
            //_window.Activate();

            //System.Diagnostics.Debug.WriteLine("OnLaunched()");

            var onLeftClickCommand = (XamlUICommand)Resources["OnLeftClickCommand"];
            onLeftClickCommand.ExecuteRequested += OnLeftClickCommand_ExecuteRequested;
            var onRightClickCommand = (XamlUICommand)Resources["OnRightClickCommand"];
            onRightClickCommand.ExecuteRequested += OnRightClickCommand_ExecuteRequested;
            var EditCommand = (XamlUICommand)Resources["EditCommand"];
            EditCommand.ExecuteRequested += EditCommand_ExecuteRequested;
            var ReloadCommand = (XamlUICommand)Resources["ReloadCommand"];
            ReloadCommand.ExecuteRequested += ReloadCommand_ExecuteRequested;
            var ExitCommand = (XamlUICommand)Resources["ExitCommand"];
            ExitCommand.ExecuteRequested += ExitCommand_ExecuteRequested;
            var HelpCommand = (XamlUICommand)(Resources["HelpCommand"]);
            HelpCommand.ExecuteRequested += HelpCommand_ExecuteRequested;
            /*var LeftClickMenuClosedCommand = (XamlUICommand)(Resources["LeftClickMenuClosed"]);
            LeftClickMenuClosedCommand.ExecuteRequested += OnLeftClickMenuClosed_ExecuteRequested;*/

            _startTrayIcon = (TaskbarIcon)Resources["StartTrayIcon"];
            _startTrayIcon.ForceCreate();

            //_startTrayIcon.ContextRequested += _startTrayIcon_ContextRequested; // TODO 2 clean up
            //_startTrayIcon.ContextCanceled += _startTrayIcon_ContextCanceled; ; // TODO 2 clean up

            //MenuFlyout fo = (MenuFlyout)_startTrayIcon.ContextFlyout;
            //fo.Opening += OnOpening;
            //_startTrayIcon.ContextFlyout.Opening += OnOpening;

            /*var newItem = new MenuFlyoutItem { Text = "SeCoNd" };
            fo.Items.Add(newItem);
            var subMenu = new MenuFlyoutSubItem { Text = "Games" };
            fo.Items.Add(subMenu);*/

            _leftClickMenu = (MenuFlyout)Resources["LeftClickMenu"];
            //_leftClickMenu.Opening += OnOpening;  // Attach handler since this menu gets swapped in as ContextFlyout
            //_leftClickMenu.Opened += OnOpening;
            _rightClickMenu = (MenuFlyout)Resources["RightClickMenu"];
            //_rightClickMenu.Opening += OnOpening;  // Attach handler since this menu can also be swapped in
            //_rightClickMenu.Opened += OnOpening;

            //_startTrayIcon.ContextFlyout = _leftClickMenu;  // TODO 2 del?
            /*EventHandler<object> LeftClickMenu_Opening = (_, _) => {
                System.Diagnostics.Debug.WriteLine("LeftClickMenuOpening()");
            };*/
            //_leftClickMenu.Opening += LeftClickMenu_Opening;
            //_leftClickMenu.Opening += OnOpening;
            //_leftClickMenu.Closing += _leftClickMenu_Closing;
            /*EventHandler<object> LeftClickMenuClosedHandler = (_, _) => {
                System.Diagnostics.Debug.WriteLine("LeftClickMenuClosedHandler()");
            };*/
            //_leftClickMenu.Closed += LeftClickMenuClosedHandler;
            /*var newItem = new MenuFlyoutItem { Text = "Poached" };
            XamlUICommand command = new();
            command.CanExecuteRequested += MenuItemCommand_CanExecuteRequested;
            command.ExecuteRequested += MenuItemCommand_ExecuteRequested;
            newItem.Command = command;
            newItem.CommandParameter = 42;  // TODO unique id to record for this item; or use newItem.Tag
            var subMenu = new MenuFlyoutSubItem { Text = "Games" };
            subMenu.Items.Add(new MenuFlyoutItem { Text = "Flight Simulator" });
            subMenu.Items.Add(newItem);
            //TrayMenu.Items.Add(new MenuFlyoutSeparator()); // Optional visual divider
            trayMenu.Items.Add(subMenu);
            subMenu = new MenuFlyoutSubItem { Text = "Work" };
            trayMenu.Items.Add(subMenu);*/

            //Menu? menu = LoadStartLeXml();
            //if (menu == null) throw new Exception("can't read menu");
            //PrintMenu(menu);

            //CreateMenu menu = new();

            var keyboardHook = new LowLevelKeyboardHook();
            keyboardHook.Handling = true;
            keyboardHook.Up += (_, args) => {
                if (args.Keys.Are(Key.LWin, Key.OemQuestion) || args.Keys.Are(Key.RWin, Key.OemQuestion)) {   // might not work on non-US keyboards; could use Key.Y
                    // TODO 5 try to refocus previous window if menu was cancelled without selecting anything
                    //System.Diagnostics.Debug.WriteLine("keyboardHook.Up()");
                    args.IsHandled = true;  // TODO 5 Oem5 keypress still goes through to running window
                    _uiQueue.TryEnqueue(() =>
                    {
                        //_startTrayIcon.ContextFlyout.Opening += ContextFlyout_Opening;
                        if (_startTrayIcon.ContextFlyout != _leftClickMenu) _startTrayIcon.ContextFlyout = _leftClickMenu;

                        GetCursorPos(out POINT point);
                        _startTrayIcon.ShowContextMenu(new System.Drawing.Point(point.X, point.Y)); // TODO 5 show at bottom-right of window??
                    });
                }
                //if (args.Keys.IsWinPressed && args.Key == Key.Divide)_startTrayIcon.ShowContextMenu();
                //System.Diagnostics.Debug.WriteLine(args.ToString());
            };
            keyboardHook.Start();

            LoadMenu();

            //_leftClickMenu.Opening += _leftClickMenu_Opening;
            //_leftClickMenu.Opened += _leftClickMenu_Opening;
            /*string? error = await CreateMenuAsync(_leftClickMenu.Items);
            if (error != null)
            {
                _leftClickMenu.Items.Clear();   // remove partially-loaded menu content

                var toast = new AppNotificationBuilder().AddText("Can't create menu.").AddText(error).BuildNotification();
                AppNotificationManager.Default.Show(toast);
                //_startTrayIcon?.Dispose();    // Presumably Exit() does this; don't Dispose() twice or there'll be an exception.
                //Exit(); // don't Exit(): use empty or null menu, and let user use right-click commands to fix it
            }*/
        }

        /*private void _startTrayIcon_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void _startTrayIcon_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }*/

        /*private void ContextFlyout_Closed(object? sender, object e)
        {
            throw new NotImplementedException();
        }*/

        /*private void OnOpening(object? sender, object e)
        {
            System.Diagnostics.Debug.WriteLine("OnOpening()");
        }*/

        /*private void _leftClickMenu_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("_leftClickMenu_Closing()");
        }*/

        private async void LoadMenu(bool toastIfOkay = false)
        {
            if (_leftClickMenu == null) {
                AppNotificationManager.Default.Show(new AppNotificationBuilder().AddText("Menu not ready!").BuildNotification());
                return;
            } 
            string? error = await CreateMenuAsync(_leftClickMenu.Items);
            if (error == null)
            {
                if (toastIfOkay)
                {
                    var toast = new AppNotificationBuilder().AddText("Menu reloaded.").BuildNotification();
                    AppNotificationManager.Default.Show(toast);
                }
            }
            else
            {
                _leftClickMenu.Items.Clear();   // remove partially-loaded menu content

                var toast = new AppNotificationBuilder().AddText("Can't load menu.").AddText(error).BuildNotification();
                AppNotificationManager.Default.Show(toast);
            }
        }

        private void OnLeftClickCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("OnLeftClickCommand_ExecuteRequested()");
            //_leftClickMenu?.Opening += _leftClickMenu_Opening;
            //_leftClickMenu?.Closing += _leftClickMenu_Closing;

            if (_startTrayIcon?.ContextFlyout != _leftClickMenu) _startTrayIcon?.ContextFlyout = _leftClickMenu;



            //_startTrayIcon?.ContextFlyout.Opening += ContextFlyout_Opening;
        }

        /*private void ContextFlyout_Opening(object? sender, object e)
        {
            System.Diagnostics.Debug.WriteLine("ContextFlyout_Opening()");
        }*/

        /*private void _leftClickMenu_Opening(object? sender, object e)
        {
            System.Diagnostics.Debug.WriteLine("_leftClickMenu_Opening()");
        }*/

        private void OnRightClickCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            if (_startTrayIcon?.ContextFlyout != _rightClickMenu) _startTrayIcon?.ContextFlyout = _rightClickMenu;
        }

        private void ReloadCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            _leftClickMenu?.Items.Clear();
            LoadMenu(true);
        }

        private void EditCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            bool useNotepad = (string)args.Parameter == "Open in Notepad";
            string startLePath = ApplicationData.Current.LocalFolder.Path + "\\Menu.xml";

            ProcessStartInfo processStartInfo = new ProcessStartInfo {
                FileName = useNotepad ? "Notepad" : startLePath,
                UseShellExecute = true
            };
            if (useNotepad) processStartInfo.Arguments = startLePath;
            ProcessStart((string)args.Parameter, processStartInfo);
        }

        private void HelpCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "https://gondwanasoftware.au/startle",
                UseShellExecute = true
            };
            ProcessStart((string)args.Parameter, processStartInfo);
        }

        private void ExitCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            Exit();
        }

        private static void ProcessStart(string name, ProcessStartInfo? startInfo)
        {
            void ShowToast(string text)
            {
                var toast = new AppNotificationBuilder().AddText("Can't start " + name).AddText(text + ' ' + startInfo.FileName).BuildNotification();
                AppNotificationManager.Default.Show(toast);
            }

            if (startInfo != null)
                try
                {
                    Process.Start(startInfo);
                }
                catch (Win32Exception e)
                {
                    switch(e.NativeErrorCode)
                    {
                        case 2:
                            ShowToast("Can't find/access/open");    // could be permission or association failure
                            break;
                        case 5:
                            ShowToast("Can't access");
                            break;
                        case 193:
                            ShowToast("Can't execute");
                            break;
                        case 1155:
                            ShowToast("No file association or unrecognised verb for");
                            break;
                        case 1223: break;   // "Operation cancelled", which isn't really an error
                        default:
                            string errorMessage = new Win32Exception(e.NativeErrorCode).Message;
                            ShowToast(errorMessage + " Check");
                            break;
                    }
                }
                catch (FileNotFoundException)
                {
                    ShowToast("Can't find");
                }
                catch (InvalidOperationException)
                {
                    ShowToast("Invalid operation (check filename and verb).");
                }
                catch (Exception e)
                {
                    ShowToast(e.Message);
                }
        }

        /*private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            if (args.Argument.Contains("install"))
            {
                // Logic to start installation
            }
        }*/

        /*public static void PrintMenu(Menu menu, int indent = 0)
        {
            System.Diagnostics.Debug.WriteLine($"{new string(' ', indent)}[{menu.Text}]");
            foreach (var item in menu.Items)
            {
                System.Diagnostics.Debug.WriteLine($"{new string(' ', indent + 2)}- {item.Text}");
            }

            foreach(var subMenu in menu.SubMenus)
            {
                PrintMenu(subMenu, indent + 4);
            }
        }*/

        /*private static void MenuItemCommand_CanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = true;
        }*/

        /*private static void MenuItemCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "steam://rungameid/1496310",
                //Arguments = "C:\\temp\\log.txt", // Optional: file to open
                UseShellExecute = true // Required in .NET Core/5+ to open files/URLs
            };

            Process.Start(startInfo);
        }*/

        private static void MenuItemCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("MenuItemCommand_ExecuteRequested()");
            ItemInfo? itemInfo = args.Parameter as ItemInfo;
            if (itemInfo == null) throw new Exception("itemInfo is null");   // shouldn't happen
            ProcessStartInfo? startInfo = itemInfo.ProcessStartInfo;
            ProcessStart(itemInfo.Text??"(unknown)", startInfo);

            /*void ShowToast(string text)
            {
                var toast = new AppNotificationBuilder().AddText("Can't start " + itemInfo.Text).AddText(text + ' ' + startInfo.FileName).BuildNotification();
                AppNotificationManager.Default.Show(toast);
            }

            if (startInfo != null)
                try
                {
                    Process.Start(startInfo);
                }
                catch(Win32Exception e) {
                    if (e.NativeErrorCode == 2) ShowToast("Can't find/access/open");    // could be permission or association failure
                    else if (e.NativeErrorCode == 5) ShowToast("Can't access");
                    else if (e.NativeErrorCode == 193) ShowToast("Can't execute");
                    //else if (e.NativeErrorCode == 1223) ShowToast("Operation cancelled"); // this isn't really an error
                }
                catch (FileNotFoundException)
                {
                    ShowToast("Can't find");
                }
                catch (InvalidOperationException)
                {
                    ShowToast("Invalid operation (check filename).");
                }
                catch (Exception e)
                {
                    ShowToast(e.Message);
                }*/
        }

        /*private void OnLeftClickMenuClosed_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {

        }*/

        /*private void MenuFlyout_Opening(object sender, object e)
        {

        }*/

        /*public class Item
        {
            public Item() {
                //System.Diagnostics.Debug.WriteLine("Constructing item");
            }

            [XmlAttribute("text")]
            public string Text {
                get;
                set;
            } = "";
        }*/

        /*[XmlRoot("menu")]
        public class Menu
        {
            private List<Menu> _subMenus = [];
            private List<Item> _items = [];

            public Menu() {
                //System.Diagnostics.Debug.WriteLine("Constructing menu");
            }

            [XmlAttribute("text")]
            public string Text { get; set; } = "";

            [XmlElement("menu")]
            public List<Menu> SubMenus
            {
                get { 
                    //System.Diagnostics.Debug.WriteLine($"Getting menu [{Text}]; subMenus={_subMenus.Count}"); 
                    return _subMenus; 
                }
                set {
                    _subMenus = value; 
                    //System.Diagnostics.Debug.WriteLine($"Setting menu [{Text}]"); 
                }
            }// = new List<Menu>();

            [XmlElement("item")]
            public List<Item> Items {
                get { 
                    //System.Diagnostics.Debug.WriteLine($"Getting item {Text}"); 
                    return _items; 
                }
                set { 
                    _items = value; 
                    //System.Diagnostics.Debug.WriteLine($"Setting item {Text}"); 
                }
            }// = new List<Item>();
        }*/
    }
}