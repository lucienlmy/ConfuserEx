using System.Threading.Tasks;

namespace ConfuserEx.ViewModel {
	public interface IUIServices {
		Task<string> OpenFolderPickerAsync(string title, string initialFolder);
	}
}
