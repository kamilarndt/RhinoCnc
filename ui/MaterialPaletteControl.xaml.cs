using RhinoCncSuite.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Material Palette Control created programmatically to ensure stability in Rhino environment.
    /// </summary>
    public class MaterialPaletteControl : UserControl
    {
        public MaterialPaletteControl(MaterialPaletteViewModel viewModel, ResourceDictionary themeResources)
        {
            DataContext = viewModel;
            
            // Make theme resources (like DataTemplates) available to this control
            if (themeResources != null)
            {
                Resources.MergedDictionaries.Add(themeResources);
            }

            // Build the UI in code
            CreateLayout();
        }

        private void CreateLayout()
        {
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var header = CreateHeader();
            Grid.SetRow(header, 0);
            mainGrid.Children.Add(header);

            var content = CreateContentArea();
            Grid.SetRow(content, 1);
            mainGrid.Children.Add(content);

            this.Content = mainGrid;
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
        }

        private UIElement CreateHeader()
        {
            var headerGrid = new Grid();
            headerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            headerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // --- Search Box ---
            var searchBox = new TextBox
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E")),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555")),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14
            };
            var searchBinding = new Binding("SearchText") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            searchBox.SetBinding(TextBox.TextProperty, searchBinding);
            Grid.SetRow(searchBox, 0);
            headerGrid.Children.Add(searchBox);

            // --- View Controls ---
            var controlsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            
            var tileButton = new Button { Content = "Kafelki", Margin = new Thickness(0, 0, 10, 0), Padding = new Thickness(10, 5, 10, 5) };
            tileButton.SetBinding(Button.CommandProperty, new Binding("SetTileViewCommand"));
            controlsPanel.Children.Add(tileButton);

            var listButton = new Button { Content = "Lista", Margin = new Thickness(0, 0, 10, 0), Padding = new Thickness(10, 5, 10, 5) };
            listButton.SetBinding(Button.CommandProperty, new Binding("SetListViewCommand"));
            controlsPanel.Children.Add(listButton);

            var sizeSlider = new Slider
            {
                Minimum = 80,
                Maximum = 200,
                Width = 100,
                VerticalAlignment = VerticalAlignment.Center
            };
            sizeSlider.SetBinding(Slider.ValueProperty, new Binding("TileSize"));
            sizeSlider.SetBinding(Slider.VisibilityProperty, new Binding("IsTileView") { Converter = new BooleanToVisibilityConverter() });
            controlsPanel.Children.Add(sizeSlider);

            Grid.SetRow(controlsPanel, 1);
            headerGrid.Children.Add(controlsPanel);

            return new Border
            {
                Child = headerGrid,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F46")),
                Padding = new Thickness(10, 10, 10, 10)
            };
        }

        private UIElement CreateContentArea()
        {
            var itemsControl = new ItemsControl();
            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Materials"));

            // --- DataTemplate dla kafelków materiałów / DataTemplate for material tiles ---
            var tileTemplate = CreateTileTemplate();
            var listTemplate = CreateListTemplate();

            // --- Style with DataTriggers to switch ItemTemplate and ItemsPanel ---
            var style = new Style(typeof(ItemsControl));

            // -- Tile View Trigger --
            var tileTrigger = new DataTrigger { Binding = new Binding("IsTileView"), Value = true };
            var tilePanelTemplate = new ItemsPanelTemplate();
            tilePanelTemplate.VisualTree = new FrameworkElementFactory(typeof(WrapPanel));
            tileTrigger.Setters.Add(new Setter(ItemsControl.ItemsPanelProperty, tilePanelTemplate));
            tileTrigger.Setters.Add(new Setter(ItemsControl.ItemTemplateProperty, tileTemplate));
            style.Triggers.Add(tileTrigger);
            
            // -- List View Trigger --
            var listTrigger = new DataTrigger { Binding = new Binding("IsTileView"), Value = false };
            var listPanelTemplate = new ItemsPanelTemplate();
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
            listPanelTemplate.VisualTree = stackPanelFactory;
            listTrigger.Setters.Add(new Setter(ItemsControl.ItemsPanelProperty, listPanelTemplate));
            listTrigger.Setters.Add(new Setter(ItemsControl.ItemTemplateProperty, listTemplate));
            style.Triggers.Add(listTrigger);

            itemsControl.Style = style;

            return new ScrollViewer
            {
                Content = itemsControl,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        /// <summary>
        /// Tworzy template dla kafelków materiałów z przyciskami operacji
        /// Creates template for material tiles with operation buttons
        /// </summary>
        private DataTemplate CreateTileTemplate()
        {
            var template = new DataTemplate();
            
            // Border główny / Main border
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#404040")));
            borderFactory.SetValue(Border.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#606060")));
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            borderFactory.SetValue(Border.MarginProperty, new Thickness(5));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(8));
            borderFactory.SetBinding(Border.WidthProperty, new Binding("TileSize") { Source = this.DataContext });
            borderFactory.SetBinding(Border.HeightProperty, new Binding("TileSize") { Source = this.DataContext });

            // Grid główny / Main grid  
            var gridFactory = new FrameworkElementFactory(typeof(Grid));
            
            // Dodaj definicje wierszy bezpośrednio / Add row definitions directly
            var row1 = new FrameworkElementFactory(typeof(RowDefinition));
            row1.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
            gridFactory.AppendChild(row1);
            
            var row2 = new FrameworkElementFactory(typeof(RowDefinition)); 
            row2.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
            gridFactory.AppendChild(row2);
            
            var row3 = new FrameworkElementFactory(typeof(RowDefinition));
            row3.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            gridFactory.AppendChild(row3);
            
            // Nazwa materiału / Material name
            var nameFactory = new FrameworkElementFactory(typeof(TextBlock));
            nameFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            nameFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            nameFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            nameFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            nameFactory.SetValue(Grid.RowProperty, 0);
            gridFactory.AppendChild(nameFactory);

            // Grubość / Thickness
            var thicknessFactory = new FrameworkElementFactory(typeof(TextBlock));
            thicknessFactory.SetBinding(TextBlock.TextProperty, new Binding("FormattedThickness"));
            thicknessFactory.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
            thicknessFactory.SetValue(Grid.RowProperty, 1);
            gridFactory.AppendChild(thicknessFactory);

            // Panel przycisków operacji / Operation buttons panel
            var buttonsPanel = CreateOperationButtonsPanel();
            buttonsPanel.SetValue(Grid.RowProperty, 2);
            gridFactory.AppendChild(buttonsPanel);

            borderFactory.AppendChild(gridFactory);
            template.VisualTree = borderFactory;
            
            return template;
        }

        /// <summary>
        /// Tworzy template dla widoku listy
        /// Creates template for list view
        /// </summary>
        private DataTemplate CreateListTemplate()
        {
            var template = new DataTemplate();
            
            // Border główny / Main border
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#404040")));
            borderFactory.SetValue(Border.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#606060")));
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(10));
            borderFactory.SetValue(Border.MarginProperty, new Thickness(2));

            // Grid z kolumnami / Grid with columns
            var gridFactory = new FrameworkElementFactory(typeof(Grid));
            
            // Dodaj definicje kolumn bezpośrednio / Add column definitions directly
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(2, GridUnitType.Star));
            gridFactory.AppendChild(col1);
            
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            gridFactory.AppendChild(col2);
            
            var col3 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col3.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            gridFactory.AppendChild(col3);
            
            var col4 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col4.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            gridFactory.AppendChild(col4);
            
            // Nazwa / Name
            var nameFactory = new FrameworkElementFactory(typeof(TextBlock));
            nameFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            nameFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            nameFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            nameFactory.SetValue(Grid.ColumnProperty, 0);
            gridFactory.AppendChild(nameFactory);

            // Grubość / Thickness
            var thicknessFactory = new FrameworkElementFactory(typeof(TextBlock));
            thicknessFactory.SetBinding(TextBlock.TextProperty, new Binding("FormattedThickness"));
            thicknessFactory.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
            thicknessFactory.SetValue(Grid.ColumnProperty, 1);
            gridFactory.AppendChild(thicknessFactory);

            // Typ / Type
            var typeFactory = new FrameworkElementFactory(typeof(TextBlock));
            typeFactory.SetBinding(TextBlock.TextProperty, new Binding("Type"));
            typeFactory.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
            typeFactory.SetValue(Grid.ColumnProperty, 2);
            gridFactory.AppendChild(typeFactory);

            // Panel przycisków / Buttons panel
            var buttonsPanel = CreateOperationButtonsPanel();
            buttonsPanel.SetValue(Grid.ColumnProperty, 3);
            gridFactory.AppendChild(buttonsPanel);

            borderFactory.AppendChild(gridFactory);
            template.VisualTree = borderFactory;
            
            return template;
        }

        /// <summary>
        /// Tworzy panel z przyciskami operacji Material Manager-a
        /// Creates panel with Material Manager operation buttons
        /// </summary>
        private FrameworkElementFactory CreateOperationButtonsPanel()
        {
            var panelFactory = new FrameworkElementFactory(typeof(StackPanel));
            panelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            panelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            // Przycisk Eye (👁) - Show/Hide
            var eyeButton = new FrameworkElementFactory(typeof(Button));
            eyeButton.SetValue(Button.ContentProperty, "👁");
            eyeButton.SetValue(Button.ToolTipProperty, "Pokaż/Ukryj obiekty z tym materiałem");
            eyeButton.SetValue(Button.MarginProperty, new Thickness(2));
            eyeButton.SetValue(Button.PaddingProperty, new Thickness(4, 2, 4, 2));
            eyeButton.SetValue(Button.FontSizeProperty, 12.0);
            eyeButton.SetBinding(Button.CommandProperty, new Binding("EyeCommand") { Source = this.DataContext });
            eyeButton.SetBinding(Button.CommandParameterProperty, new Binding("."));
            panelFactory.AppendChild(eyeButton);

            // Przycisk Brush (🖌) - Assign material
            var brushButton = new FrameworkElementFactory(typeof(Button));
            brushButton.SetValue(Button.ContentProperty, "🖌");
            brushButton.SetValue(Button.ToolTipProperty, "Przypisz materiał do zaznaczonych obiektów");
            brushButton.SetValue(Button.MarginProperty, new Thickness(2));
            brushButton.SetValue(Button.PaddingProperty, new Thickness(4, 2, 4, 2));
            brushButton.SetValue(Button.FontSizeProperty, 12.0);
            brushButton.SetBinding(Button.CommandProperty, new Binding("BrushCommand") { Source = this.DataContext });
            brushButton.SetBinding(Button.CommandParameterProperty, new Binding("."));
            panelFactory.AppendChild(brushButton);

            // Przycisk Target (🎯) - Select objects
            var targetButton = new FrameworkElementFactory(typeof(Button));
            targetButton.SetValue(Button.ContentProperty, "🎯");
            targetButton.SetValue(Button.ToolTipProperty, "Zaznacz wszystkie obiekty z tym materiałem");
            targetButton.SetValue(Button.MarginProperty, new Thickness(2));
            targetButton.SetValue(Button.PaddingProperty, new Thickness(4, 2, 4, 2));
            targetButton.SetValue(Button.FontSizeProperty, 12.0);
            targetButton.SetBinding(Button.CommandProperty, new Binding("TargetCommand") { Source = this.DataContext });
            targetButton.SetBinding(Button.CommandParameterProperty, new Binding("."));
            panelFactory.AppendChild(targetButton);

            // Przycisk Lock (🔒) - Lock/unlock objects
            var lockButton = new FrameworkElementFactory(typeof(Button));
            lockButton.SetValue(Button.ContentProperty, "🔒");
            lockButton.SetValue(Button.ToolTipProperty, "Zablokuj/odblokuj obiekty z tym materiałem");
            lockButton.SetValue(Button.MarginProperty, new Thickness(2));
            lockButton.SetValue(Button.PaddingProperty, new Thickness(4, 2, 4, 2));
            lockButton.SetValue(Button.FontSizeProperty, 12.0);
            lockButton.SetBinding(Button.CommandProperty, new Binding("LockCommand") { Source = this.DataContext });
            lockButton.SetBinding(Button.CommandParameterProperty, new Binding("."));
            panelFactory.AppendChild(lockButton);

            // Przycisk Plus (➕) - Insert geometry
            var plusButton = new FrameworkElementFactory(typeof(Button));
            plusButton.SetValue(Button.ContentProperty, "➕");
            plusButton.SetValue(Button.ToolTipProperty, "Wstaw geometrię materiału");
            plusButton.SetValue(Button.MarginProperty, new Thickness(2));
            plusButton.SetValue(Button.PaddingProperty, new Thickness(4, 2, 4, 2));
            plusButton.SetValue(Button.FontSizeProperty, 12.0);
            plusButton.SetBinding(Button.CommandProperty, new Binding("PlusCommand") { Source = this.DataContext });
            plusButton.SetBinding(Button.CommandParameterProperty, new Binding("."));
            panelFactory.AppendChild(plusButton);

            return panelFactory;
        }
    }
} 