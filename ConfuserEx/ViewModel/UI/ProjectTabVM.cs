using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Confuser.Core.Project;
using ConfuserEx.Views;
#if NET
using CommunityToolkit.Mvvm.Input;
#else
using GalaSoft.MvvmLight.CommandWpf;
#endif
using Ookii.Dialogs.Wpf;

namespace ConfuserEx.ViewModel {
	public class ProjectTabVM : TabViewModel {
		private readonly IUIServices uiServices;

		public ProjectTabVM(AppVM app, IUIServices uiServices)
			: base(app, "Project") {
			this.uiServices = uiServices;
		}

		public ICommand DragDrop {
			get {
				return new RelayCommand<IDataObject>(data => {
					foreach (string file in (string[])data.GetData(DataFormats.FileDrop))
						AddModule(file);
				}, data => {
					if (!data.GetDataPresent(DataFormats.FileDrop))
						return false;
					var files = (string[])data.GetData(DataFormats.FileDrop);
					bool ret = files.All(file => File.Exists(file));
					return ret;
				});
			}
		}

		public ICommand ChooseBaseDir {
			get {
#if NET
				return new AsyncRelayCommand(async () => {
					var selectedPath = await uiServices.OpenFolderPickerAsync("Select base directory", App.Project.BaseDirectory);
					if (selectedPath != null) {
						App.Project.BaseDirectory = selectedPath;
						App.Project.OutputDirectory = Path.Combine(selectedPath, "Confused");
					}
				});
#else
				return new RelayCommand(() => {
					var selectedPath = uiServices.OpenFolderPickerAsync("Select base directory", App.Project.BaseDirectory).Result;
					if (selectedPath != null) {
						App.Project.BaseDirectory = selectedPath;
						App.Project.OutputDirectory = Path.Combine(selectedPath, "Confused");
					}
				});
#endif
			}
		}

		public ICommand ChooseOutputDir {
			get {
#if NET
				return new AsyncRelayCommand(async () => {
					var selectedPath = await uiServices.OpenFolderPickerAsync("Select output directory", App.Project.OutputDirectory);
					if (selectedPath != null) {
						App.Project.OutputDirectory = selectedPath;
					}
				});
#else
				return new RelayCommand(() => {
					var selectedPath = uiServices.OpenFolderPickerAsync("Select output directory", App.Project.OutputDirectory).Result;
					if (selectedPath != null) {
						App.Project.OutputDirectory = selectedPath;
					}
				});
#endif
			}
		}

		public ICommand Add {
			get {
				return new RelayCommand(() => {
					var ofd = new VistaOpenFileDialog();
					ofd.Filter = ".NET assemblies (*.exe, *.dll)|*.exe;*.dll|All Files (*.*)|*.*";
					ofd.Multiselect = true;
					if (ofd.ShowDialog() ?? false) {
						foreach (var file in ofd.FileNames)
							AddModule(file);
					}
				});
			}
		}

		public ICommand Remove {
			get {
				return new RelayCommand(() => {
					Debug.Assert(App.Project.Modules.Any(m => m.IsSelected));
					string msg = "Are you sure to remove selected modules?\r\nAll settings specific to it would be lost!";
					if (MessageBox.Show(msg, "ConfuserEx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
						foreach (var item in App.Project.Modules.Where(m => m.IsSelected).ToList())
							App.Project.Modules.Remove(item);
					}
				}, () => App.Project.Modules.Any(m => m.IsSelected));
			}
		}

		public ICommand Edit {
			get {
				return new RelayCommand<ProjectModuleVM>(module => {
					if (module is null) {
						Debug.Assert(App.Project.Modules.Count(m => m.IsSelected) == 1);
						module = App.Project.Modules.Single(m => m.IsSelected);
					}
					var dialog = new ProjectModuleView(module);
					dialog.Owner = Application.Current.MainWindow;
					dialog.ShowDialog();
				}, module => !(module is null) || App.Project.Modules.Count(m => m.IsSelected) == 1);
			}
		}

		public ICommand Advanced {
			get {
				return new RelayCommand(() => {
					var dialog = new ProjectTabAdvancedView(App.Project);
					dialog.Owner = Application.Current.MainWindow;
					dialog.ShowDialog();
				});
			}
		}

		void AddModule(string file) {
			if (!File.Exists(file)) {
				MessageBox.Show(string.Format("File '{0}' does not exists!", file), "ConfuserEx", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (string.IsNullOrEmpty(App.Project.BaseDirectory)) {
				string directory = Path.GetDirectoryName(file);
				App.Project.BaseDirectory = directory;
				App.Project.OutputDirectory = Path.Combine(directory, "Confused");
			}
			var module = new ProjectModuleVM(App.Project, new ProjectModule());
			try {
				module.Path = Confuser.Core.Utils.GetRelativePath(file, App.Project.BaseDirectory) ?? file;
			}
			catch {
				module.Path = file;
			}
			App.Project.Modules.Add(module);
		}
	}
}
