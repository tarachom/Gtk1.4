using Gtk;
using Npgsql;

namespace GtkTest
{
    class WindowStart : Window
    {
        CancellationTokenSource? CancellationTokenThread { get; set; }

        NpgsqlDataSource? DataSource { get; set; }

        TreeStore? Store;
        TreeView? treeView;
        ScrolledWindow scrollTerminal;
        TextView textTerminal;

        Button bOk;
        Button bStop;

        public WindowStart() : base("PostgreSQL + GTKSharp")
        {
            SetDefaultSize(1600, 900);
            SetPosition(WindowPosition.Center);

            DeleteEvent += delegate { Program.Quit(); };

            VBox vbox = new VBox();
            Add(vbox);

            #region Кнопки

            //Кнопки
            HBox hBoxButton = new HBox();
            vbox.PackStart(hBoxButton, false, false, 10);

            Button bConnect = new Button("Підключитись до PostgreSQL");
            bConnect.Clicked += OnConnect;
            hBoxButton.PackStart(bConnect, false, false, 10);

            Button bFill = new Button("Заповнити");
            bFill.Clicked += OnFill;
            hBoxButton.PackStart(bFill, false, false, 10);

            bOk = new Button("Оптимізувати");
            bOk.Clicked += OnOkClick;
            hBoxButton.PackStart(bOk, false, false, 10);

            bStop = new Button("Зупинити") { Sensitive = false };
            bStop.Clicked += OnStopClick;
            hBoxButton.PackStart(bStop, false, false, 10);

            #endregion

            HPaned hPaned = new HPaned();
            hPaned.Position = 600;

            vbox.PackStart(hPaned, true, true, 0);

            //Список
            HBox hboxTree = new HBox();

            AddColumn();

            ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(treeView);

            hboxTree.PackStart(scroll, true, true, 10);

            hPaned.Pack1(hboxTree, false, true);

            //Terminal
            HBox hBoxTerminal = new HBox();

            scrollTerminal = new ScrolledWindow();
            scrollTerminal.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollTerminal.Add(textTerminal = new TextView());

            hBoxTerminal.PackStart(scrollTerminal, true, true, 5);

            hPaned.Pack2(hBoxTerminal, false, true);

            //statusBar
            Statusbar statusBar = new Statusbar();
            vbox.PackStart(statusBar, false, false, 0);

            ShowAll();
        }

        void AddColumn()
        {
            Store = new TreeStore(typeof(string), typeof(string), typeof(string));
            treeView = new TreeView(Store);

            treeView.AppendColumn(new TreeViewColumn("Дерево", new CellRendererText(), "text", 0));
            treeView.AppendColumn(new TreeViewColumn("Тип", new CellRendererText(), "text", 1));
            treeView.AppendColumn(new TreeViewColumn("Udt", new CellRendererText(), "text", 2));
        }

        void OnConnect(object? sender, EventArgs args)
        {
            string Server = "localhost";
            string UserId = "postgres";
            string Password = "1";
            int Port = 5432;
            string Database = "storage_and_trade";

            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};Database={Database};SSLMode=Prefer;";

            DataSource = NpgsqlDataSource.Create(conString);

            OnFill(this, new EventArgs());
        }

        void OnFill(object? sender, EventArgs args)
        {
            if (DataSource != null)
            {
                Store!.Clear();

                PostgreSQl postgreSQl = new PostgreSQl() { DataSource = DataSource };

                //Структура бази даних
                ConfigurationInformationSchema informationSchema = postgreSQl.SelectInformationSchema();

                TreeIter rootIter = Store.AppendValues(" Схема ");

                foreach (ConfigurationInformationSchema_Table table in informationSchema.Tables.Values)
                {
                    TreeIter IterTable = Store.AppendValues(rootIter, table.TableName);

                    foreach (ConfigurationInformationSchema_Column column in table.Columns.Values)
                    {
                        TreeIter IterColumn = Store.AppendValues(IterTable, column.ColumnName, column.DataType, column.UdtName);
                    }

                    if (table.Indexes.Count != 0)
                    {
                        TreeIter IterIndex = Store.AppendValues(IterTable, "[ Індекси ] ");
                        foreach (ConfigurationInformationSchema_Index index in table.Indexes.Values)
                        {
                            TreeIter IterIndexItem = Store.AppendValues(IterIndex, index.IndexName);
                        }
                    }
                }

                treeView!.ExpandAll();
            }
        }

        void OnOkClick(object? sender, EventArgs args)
        {
            CancellationTokenThread = new CancellationTokenSource();
            Thread thread = new Thread(new ThreadStart(MaintenanceTable));
            thread.Start();
        }

        void OnStopClick(object? sender, EventArgs args)
        {
            CancellationTokenThread?.Cancel();
        }

        void ButtonSensitive(bool sensitive)
        {
            Gtk.Application.Invoke
            (
                delegate
                {
                    bOk.Sensitive = sensitive;

                    textTerminal.Sensitive = sensitive;

                    bStop.Sensitive = !sensitive;
                }
            );
        }

        void ApendLine(string text)
        {
            Gtk.Application.Invoke
            (
                delegate
                {
                    textTerminal.Buffer.InsertAtCursor(text + "\n");
                    scrollTerminal.Vadjustment.Value = scrollTerminal.Vadjustment.Upper;
                }
            );
        }

        void ClearListBoxTerminal()
        {
            Gtk.Application.Invoke
            (
                delegate
                {
                    textTerminal.Buffer.Text = "";
                }
            );
        }

        void MaintenanceTable()
        {
            if (DataSource != null)
            {
                ButtonSensitive(false);
                ClearListBoxTerminal();

                ApendLine("Структура бази даних");

                PostgreSQl postgreSQl = new PostgreSQl() { DataSource = DataSource };
                ConfigurationInformationSchema informationSchema = postgreSQl.SelectInformationSchema();

                ApendLine("Таблиць: " + informationSchema.Tables.Count);
                ApendLine("");

                ApendLine("Обробка таблиць:");

                foreach (ConfigurationInformationSchema_Table table in informationSchema.Tables.Values)
                {
                    if (CancellationTokenThread!.IsCancellationRequested)
                        break;

                    ApendLine($" --> {table.TableName}");

                    string query = $@"VACUUM FULL {table.TableName};";

                    NpgsqlCommand command = DataSource.CreateCommand(query);
                    command.ExecuteNonQuery();
                }

                ApendLine("");
                ApendLine("Готово!");

                ButtonSensitive(true);
            }
        }
    }
}
