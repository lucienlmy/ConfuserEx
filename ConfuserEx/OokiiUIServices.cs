using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfuserEx.ViewModel;
using Ookii.Dialogs.Wpf;

namespace ConfuserEx {
	internal class OokiiUIServices : IUIServices {
		public Task<string> OpenFolderPickerAsync(string title, string initialFolder) {
			var fbd = new VistaFolderBrowserDialog();
			fbd.Description = title;
			if (initialFolder != null) {
				fbd.SelectedPath = initialFolder;
			}

			if (fbd.ShowDialog() ?? false) {
				return Task.FromResult(fbd.SelectedPath);
			}

			return Task.FromResult<string>(null);
		}
	}
}
