using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Net.ServerSentEvents;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Text;
using static StartLE.App;
//using static System.Net.Mime.MediaTypeNames;

namespace StartLE
{
    public class ItemInfo
    {
        public string? Text;
        public ProcessStartInfo? ProcessStartInfo;
    }

    public partial class App : Application
    {
        private static async Task<string?> CreateMenuAsync(IList<MenuFlyoutItemBase> items)
        // Returns error or null
        {
            //string userDocsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataFolder = ApplicationData.Current.LocalFolder.Path;
            string startLePath = appDataFolder + "\\Menu.xml";

            async Task<string?> CreateMenuFileAsync()
            // Copies default menu from project assets to user's AppData folder.
            // Returns error or null.
            {
                try
                {
                    Uri assetUri = new Uri($"ms-appx:///Assets/Menu.xml");
                    StorageFile assetFile = await StorageFile.GetFileFromApplicationUriAsync(assetUri);
                    StorageFolder destFolder = await StorageFolder.GetFolderFromPathAsync(appDataFolder);
                    await assetFile.CopyAsync(destFolder, assetFile.Name, NameCollisionOption.FailIfExists);
                }
                catch(Exception e)
                {
                    return e.Message;
                }
                return null;
            }

            if (File.Exists(startLePath) == false) {
                if (Directory.Exists(appDataFolder) == false) {
                    try
                    {
                        Directory.CreateDirectory(appDataFolder);
                    }
                    catch (Exception e) { return e.Message; }
                }
                var error = await CreateMenuFileAsync();
                if (error != null) return error;
            }

            try
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    IgnoreComments = true
                    //Async = true // Enables use of ReadAsync()
                };
                string? error = "<startle> element not found.";
                using XmlReader reader = XmlReader.Create(startLePath, readerSettings);
                while (reader.Read())   // skip over <?xml ... />
                {
                    //System.Diagnostics.Debug.WriteLine($"CreateMenu: type={reader.NodeType}");
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        //System.Diagnostics.Debug.WriteLine($"Element: {reader.Name}");
                        if (reader.Name == "startle")
                            error = ParseStartle(reader, items);
                    }
                }
                return error;
            }
            catch (DirectoryNotFoundException) { return appDataFolder + " folder not found."; }
            catch (FileNotFoundException) { return startLePath + " not found."; }
            catch (XmlException e) { return e.Message; }
        }

        private static string? ParseStartle(XmlReader reader, IList<MenuFlyoutItemBase> items)
        // Parse <startle ... />
        {
            static async void DoStartup(bool enable)
            {
                StartupTask startupTask = await StartupTask.GetAsync("StartupId");
                switch (startupTask.State) {
                    case StartupTaskState.Disabled:
                        if (enable) await startupTask.RequestEnableAsync();
                        break;
                    case StartupTaskState.Enabled:
                        if (!enable) startupTask.Disable();
                        break;
                }
            }

            var version = reader.GetAttribute("version");
            var startup = reader.GetAttribute("startup");
            DoStartup(startup != null);
            reader.Read();
            if (reader.Name == "menu") return ParseMenu(reader, items, true);
            else return "<startle> must contain <menu>.";
        }

        private static string? ParseMenu(XmlReader reader, IList<MenuFlyoutItemBase> items, bool root) // We found a <menu>; read it until its EndElement
        // reader: positioned at <menu> start tag
        {
            if (root)
            {
                if (reader.AttributeCount > 0) return "Root <menu> shouldn't have any attributes.";
            } else
            {
                string? text = null, icon = null, key = null;
                while (reader.MoveToNextAttribute())
                {
                    //Console.WriteLine($"Attribute: {reader.Name} = {reader.Value}");
                    switch (reader.Name)
                    {
                        case "text": text = reader.Value; break;
                        case "icon": icon = reader.Value; break;
                        case "key": key = reader.Value; break;
                        default: return "Invalid attribute in <menu>: " + reader.Name;
                    }
                }
                if (text == null) return "A <menu> has no text attribute.";

                reader.MoveToElement();

                var subMenu = new MenuFlyoutSubItem();
                subMenu.Text = AddTextAndKey(text, key, subMenu);
                // TODO 5 icon in <menu>
                //System.Diagnostics.Debug.WriteLine($"  <menu> text: [{text}]");
                items.Add(subMenu);
                items = subMenu.Items;
            }

            string? error = null;
            reader.Read();  // skip past <menu> to first sub-element, or </menu>
            if (reader.NodeType == XmlNodeType.EndElement)  // empty menu
            {
                reader.Read();  // skip over the EndElement so reader is pointing to the next node
                return null;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                //System.Diagnostics.Debug.WriteLine($"ParseMenu: type={reader.NodeType}");

                if (reader.NodeType == XmlNodeType.Element)
                {
                    //System.Diagnostics.Debug.WriteLine($"Element: {reader.Name}");

                    switch (reader.Name)
                    {
                        case "menu":
                            error = ParseMenu(reader, items, false);
                            if (error != null) return error;
                            break;
                        case "item":
                            error = ParseItem(reader, items);
                            if (error != null) return error;
                            reader.Read();
                            break;
                        case "separator":
                            items.Add(new MenuFlyoutSeparator());
                            reader.Read();
                            break;
                        default:
                            return "Invalid element: <" + reader.Name + ">.";

                    }
                }
                else if (reader.NodeType == XmlNodeType.Text) return "Text node not allowed: " + reader.Value;
            }

            reader.Read();  // skip over the EndElement so reader is pointing to the next node

            //System.Diagnostics.Debug.WriteLine($"  Finished reading <menu> text: [{text}]");
            return null;
        }

        /*public static async Task<IconElement> CreateIconFromFileAsync(string filePath)
        {
            // 1. Get the file from the local path
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                // 2. Decode the .ico file
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // 3. Convert to a format WinUI can display (BGRA8 with premultiplied alpha)
                SoftwareBitmap displayBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);



                softwareBitmap = SoftwareBitmap.Convert(displayBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
                await bitmapSource.SetBitmapAsync(softwareBitmap);
                return new ImageIcon { Source = bitmapSource };



                // 4. Create the Source
                SoftwareBitmapSource source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(displayBitmap);

                return new ImageIcon { Source = source };

                // 5. Return as a BitmapIcon
                return new BitmapIcon()
                {
                    // Note: Since WinUI 3, BitmapIcon.UriSource is the standard property,
                    // but for custom streams, we often use a specialized approach 
                    // depending on the specific control needs.
                    ShowAsMonochrome = false
                };
            }
        }*/

        /*async static void setIcon(MenuFlyoutItem item, string filePath)
        {
            //var iconElement = await CreateIconFromFileAsync("file:///c:/Users/Peter/.startle/Steam.ico");  // TODO 5 don't hard-code .xml folder (anywhere)
            //var iconElement = new BitmapIcon { UriSource = new Uri(filePath), ShowAsMonochrome = false };
            //var iconElement = new BitmapIcon { UriSource = new Uri("ms-appx:///Assets/StartLE.ico"), ShowAsMonochrome = false };
            //item.Icon = iconElement;
            //item.Icon = new SymbolIcon(Symbol.Refresh);
            //item.Icon = new BitmapIcon { UriSource = new Uri("ms-appx:///Assets/StoreLogo.png") };

            var ras = File.OpenRead("c:\\Users\\Peter\\.startle\\Steam.ico").AsRandomAccessStream();  // TODO 5 icon test: don't hard-code .xml folder (anywhere)
            var bmp = new BitmapImage();
            await bmp.SetSourceAsync(ras);
            var icon = new ImageIcon { Source = bmp };
            item.Icon = icon;
        }*/

        private static string? ParseItem(XmlReader reader, IList<MenuFlyoutItemBase> items) // We found a <item>; read it
        {
            static ProcessWindowStyle? ParseStyle(string name)
            {
                return Enum.TryParse(name, true, out ProcessWindowStyle selectedStyle)? selectedStyle: null;
            }

            string? text = null, filename = null, icon = null, key = null, folder = null, args = null, verb = null;
            ProcessWindowStyle? style = null;

            while (reader.MoveToNextAttribute())
            {
                //Console.WriteLine($"Attribute: {reader.Name} = {reader.Value}");
                switch (reader.Name)
                {
                    case "text": text = reader.Value; break;
                    case "filename": filename = reader.Value; break;
                    case "icon": icon = reader.Value; break;
                    case "key": key = reader.Value; break;
                    case "folder": folder = reader.Value; break;
                    case "args": args = reader.Value; break;
                    case "verb": verb = reader.Value; break;
                    case "style": 
                        style = ParseStyle(reader.Value);
                        if (style == null) return "Invalid style: " + reader.Value;
                        break;
                    default: return "Invalid attribute in <item>: " + reader.Name;
                }
            }
            if (text == null) return "An <item> has no text attribute.";
            if (filename == null) return "An <item> has no filename attribute.";

            reader.MoveToElement();

            var newItem = new MenuFlyoutItem();
            newItem.Text = AddTextAndKey(text, key, newItem);
            //string? text = reader.GetAttribute("text");
            //System.Diagnostics.Debug.WriteLine($"  <item> text: {text}");   // TODO 5 hide all debug output (or check it doesn't happen in release build)

            /*if (icon != null)   // TODO 5 icon! may be impossible
            {
                //var iconElement = await CreateIconFromFileAsync("c:\\Users\\Peter\\.startle\\Steam.ico"); // TODO 5 don't hard-code path
                //var bitmapIcon = new BitmapIcon { UriSource = uri, ShowAsMonochrome = false };
                //newItem.Icon = bitmapIcon;
                //setIcon(newItem, "file:///c:/Users/Peter/.startle/utilities.ico");  // TODO 5 don't hard-code .xml folder (anywhere)
            }*/
            /*if (accesskey != null)
            {
                newItem.AccessKey = accesskey;
                newItem.KeyboardAcceleratorTextOverride = accesskey;
                text = text + " (" + accesskey + ")";
            }*/
            //newItem.Text = text;

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = true // Required in .NET Core/5+ to open files/URLs
            };
            if (folder != null) startInfo.WorkingDirectory = folder;
            if (args != null) startInfo.Arguments = args;
            if (verb != null) startInfo.Verb = verb;
            if (style != null) startInfo.WindowStyle = (ProcessWindowStyle)style;
            //newItem.Tag = startInfo;
            var itemInfo = new ItemInfo { Text = newItem.Text, ProcessStartInfo = startInfo }; 

            XamlUICommand command = new();
            //command.CanExecuteRequested += (sender, args) => { args.CanExecute = true; };
            command.ExecuteRequested += MenuItemCommand_ExecuteRequested;
            //command.IconSource = new SymbolIconSource { Symbol = Symbol.ClosePane};
            newItem.Command = command;
            newItem.CommandParameter = itemInfo;

            items.Add(newItem);

            return null;
        }

        private static string AddTextAndKey(string text, string? key, MenuFlyoutItemBase entry)
        {
            if (key != null)
            {
                entry.AccessKey = key;
                //entry.KeyboardAcceleratorTextOverride = accesskey;    // doesn't work
                text = text + " (" + key + ")";
            }
            return text;
        }

        /*public class CreateMenu
        {
            public CreateMenu()
            {
                using (XmlReader reader = XmlReader.Create("C:\\Users\\Peter\\.StartLE\\StartLE.xml"))  // TODO 5 don't hard-code .xml folder
                {
                    while (reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine($"CreateMenu: type={reader.NodeType}");

                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            System.Diagnostics.Debug.WriteLine($"Element: {reader.Name}");

                            if (reader.Name == "menu") ParseMenu(reader);
                        }
                    }
                }
            }

            private static void ParseMenu(XmlReader reader) // We found a <menu>; read it until its EndElement
            {
                // read other <menu> attributes (eg, icon? accel?)
                string? text = reader.GetAttribute("text");
                System.Diagnostics.Debug.WriteLine($"  <menu> text: [{text}]");

                do
                {
                    reader.Read();

                    System.Diagnostics.Debug.WriteLine($"ParseMenu: type={reader.NodeType}");

                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        System.Diagnostics.Debug.WriteLine($"Element: {reader.Name}");

                        switch (reader.Name)
                        {
                            case "menu":
                                ParseMenu(reader); break;
                            case "item":
                                ParseItem(reader); break;

                        }
                    }
                } while (reader.NodeType != XmlNodeType.EndElement);   // TODO don't crash if no end tag

                reader.Read();  // skip over the EndElement so reader is pointing to the next node

                System.Diagnostics.Debug.WriteLine($"  Finished reading <menu> text: [{text}]");
            }

            private static void ParseItem(XmlReader reader) // We found a <item>; read it
            {
                string? text = reader.GetAttribute("text");
                System.Diagnostics.Debug.WriteLine($"  <item> text: {text}");
            }
        }*/
    }
}