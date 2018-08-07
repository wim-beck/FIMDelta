using FimDelta.Xml;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FimDelta
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly CollectionViewSource view;
		private Delta delta;
		private DeltaViewController deltaViewController;

		public MainWindow()
		{
			InitializeComponent();

			view = (CollectionViewSource)FindResource("ObjectsView");

			Loaded += new RoutedEventHandler(MainWindow_Loaded);
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				if (Environment.GetCommandLineArgs().Length > 1)
				{
					delta = new Delta(Environment.GetCommandLineArgs()[1]);
					deltaViewController = new DeltaViewController(delta);
					view.Source = deltaViewController.View;
				}
				else
				{
					OpenFileDialog fileDialog = new OpenFileDialog();
					fileDialog.Title = "Provide a changes.xml file";
					fileDialog.Filter = "XML files|*.xml";
					bool? fileChosen = fileDialog.ShowDialog();
					if (fileChosen.Value)
					{
						delta = new Delta(fileDialog.FileName);
						deltaViewController = new DeltaViewController(delta);
						view.Source = deltaViewController.View;
					}
					else
					{
						MessageBox.Show("Provide a delta file as argument.", "Warning");
						Application.Current.Shutdown();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}
		}

		private void sortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (view == null || deltaViewController == null) return;

			int sortType = ((ComboBox)sender).SelectedIndex;

			if (sortType == 1)
				deltaViewController.Grouping = GroupType.State;
			else if (sortType == 2)
				deltaViewController.Grouping = GroupType.ObjectType;
			else
				deltaViewController.Grouping = GroupType.None;

			view.Source = deltaViewController.View;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string outFile = Environment.GetCommandLineArgs()[1].Replace(".xml", "2.xml");
			if (Environment.GetCommandLineArgs().Length > 2)
			{
				outFile = Environment.GetCommandLineArgs()[2];
			}
			delta.Save(outFile);
			MessageBox.Show("Delta saved to " + outFile);
		}
	}
}
