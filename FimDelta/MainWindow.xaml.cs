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
		private string changesFile;

		/// <summary>
		/// 
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			view = (CollectionViewSource)FindResource("ObjectsView");

			Loaded += new RoutedEventHandler(MainWindow_Loaded);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				if (Environment.GetCommandLineArgs().Length > 1)
				{
					changesFile = Environment.GetCommandLineArgs()[1];
				}
				else
				{
					OpenFileDialog fileDialog = new OpenFileDialog();
					fileDialog.Title = "Provide a changes.xml file";
					fileDialog.Filter = "XML files|*.xml";
					bool? fileChosen = fileDialog.ShowDialog();
					if (fileChosen.Value)
					{
						changesFile = fileDialog.FileName;
					}
					else
					{
						MessageBox.Show("Provide a delta file as argument.", "Warning");
						Application.Current.Shutdown();
					}
				}
				delta = new Delta(changesFile);
				deltaViewController = new DeltaViewController(delta);
				view.Source = deltaViewController.View;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Possible to expand implementation with messagebox asking how to name new changes file.
			string outFile = changesFile.Replace(".xml", "2.xml");
			if (Environment.GetCommandLineArgs().Length > 2)
			{
				outFile = Environment.GetCommandLineArgs()[2];
			}
			delta.Save(outFile);
			MessageBox.Show("Delta saved to " + outFile);
		}
	}
}
