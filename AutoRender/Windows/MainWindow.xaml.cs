using AutoRender.Messaging;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using WebSocketMessaging;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;

namespace AutoRender {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : BaseWindow {
		protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private MainViewModel _objViewModel = new MainViewModel();
		private Connection _objConnection;

		public MainWindow() : base() {
			this.DataContext = _objViewModel;
			InitializeComponent();

			base.StatusChanged += delegate (WindowStatus pStatus, string pMessage) {
				_objViewModel.SetStatus(pStatus, pMessage);
			};
			Connect();
		}

		private void Connect() {
			ThreadPool.QueueUserWorkItem(state => {
				SetLoading("Loading Projects...");
				_objConnection = new Connection();
				_objConnection.Ready += _objConnection_Ready;
				_objConnection.Disconnected += _objConnection_Disconnected;
				_objConnection.Start();

				Messaging.Action.Event.WorkspaceUpdated.Updated += WorkspaceUpdated_Updated;
				Messaging.Action.Notification.WorkspaceUpdated.Updated += WorkspaceUpdated_Updated;
			});
		}

		/// <summary>
		/// When the connection has been set up, subscribe and get the status 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _objConnection_Ready(object sender, EventArgs e) {
			Reload();
		}

		private void _objConnection_Disconnected(object sender, EventArgs e) {
			this.SetLoading("Connection lost, reconnecting...");
		}

		private void UpdateStatus() {
			ThreadPool.QueueUserWorkItem((state) => {
				Messaging.Response.GetStatus objGetStatusResp = null;
				do {
					objGetStatusResp = _objConnection.Request<AutoRender.Messaging.Response.GetStatus>(new Messaging.Request.GetStatus());
					if (objGetStatusResp.Status != ResponseCode.Success) {
						if (objGetStatusResp.Status == ResponseCode.Disconnect) {
							return;
						}
						Thread.Sleep(2000);
					}
				} while (objGetStatusResp != null && objGetStatusResp.Status != ResponseCode.Success);

				if (objGetStatusResp != null) {
					_objViewModel.Clear();
					_objViewModel.Update(objGetStatusResp.WorkspaceItems);
				}
			});
		}
		private void WorkspaceUpdated_Updated(object sender, List<WorkspaceItemUpdate> pUpdates) {
			ThreadPool.QueueUserWorkItem(state => {
				foreach (var objUpdate in pUpdates) {
					switch (objUpdate.Action) {
						case WorkspaceItemUpdate.WorkspaceAction.New:
						case WorkspaceItemUpdate.WorkspaceAction.Updated:
							_objViewModel.Update(objUpdate.WorkspaceItem);
							break;
						case WorkspaceItemUpdate.WorkspaceAction.Deleted:
							_objViewModel.Delete(objUpdate.WorkspaceItem);
							break;
					}
				}
			});
		}

		private void btnStart_Click(object sender, RoutedEventArgs e) {
			ThreadPool.QueueUserWorkItem(state => {
				var lstWsItems = _objViewModel.WorkspaceItems.Where(i => i.SelectedForHandling);
				if (lstWsItems.Count() > 0) {
					foreach (var objWsItem in lstWsItems) {
						var objResp = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Request.JobStart(objWsItem.ID));
						//deselect when we were able to queue the job
						if (objResp.Status == ResponseCode.Success) {
							objWsItem.SelectedForHandling = false;
						}
					}
				}
			});
		}

		private void TargetNameChanged(object sender, RoutedEventArgs e) {
			var objViewModel = ((sender as System.Windows.Controls.Button).BindingGroup.Owner as System.Windows.Controls.DataGridRow).DataContext as WorkspaceItemViewModel;
			if (!String.IsNullOrEmpty(objViewModel.TargetName)) {
				ThreadPool.QueueUserWorkItem((state) => {
					objViewModel.IsUpdating = true;

					var objResult = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Request.UpdateProjectTarget(objViewModel.ID, objViewModel.TargetName));
					if (objResult.Status != ResponseCode.Success) {
						MessageBox.Show("Failed updating name, please try again or contact Nico the almighty");
					}
					var objGetStatusResponse = _objConnection.Request<Messaging.Response.GetStatus>(new Messaging.Request.GetStatus(objViewModel.ID.ToString()));
					if (objGetStatusResponse.Status == ResponseCode.Success && objGetStatusResponse.WorkspaceItems.Count == 1) {
						_objViewModel.Update(objGetStatusResponse.WorkspaceItems[0]);
					} else {
						MessageBox.Show("Failed reloading item, please try again or contact Nico the almighty");
					}
					objViewModel.IsUpdating = false;
				});
			}
		}

		private void SourceNameChanged(object sender, RoutedEventArgs e) {
			var objViewModel = ((sender as System.Windows.Controls.Button).BindingGroup.Owner as System.Windows.Controls.DataGridRow).DataContext as WorkspaceItemViewModel;
			if (!String.IsNullOrEmpty(objViewModel.SourceName)) {
				ThreadPool.QueueUserWorkItem((state) => {
					objViewModel.IsUpdating = true;
					var objResult = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Request.UpdateProjectSource(objViewModel.ID, objViewModel.SourceName));
					if (objResult.Status != ResponseCode.Success) {
						MessageBox.Show("Failed updating name, please try again or contact Nico the almighty");
					}
					var objGetStatusResponse = _objConnection.Request<Messaging.Response.GetStatus>(new Messaging.Request.GetStatus(objViewModel.ID.ToString()));
					if (objGetStatusResponse.Status == ResponseCode.Success && objGetStatusResponse.WorkspaceItems.Count == 1) {
						_objViewModel.Update(objGetStatusResponse.WorkspaceItems[0]);
					} else {
						MessageBox.Show("Failed reloading item, please try again or contact Nico the almighty");
					}
					objViewModel.IsUpdating = false;
				});
			}
		}

		private void btnRefresh_Click(object sender, RoutedEventArgs e) {
			ThreadPool.QueueUserWorkItem((state) => {
				UpdateStatus();
				SetLoading("Reloading server data...");
				var objResp = _objConnection.Request<AutoRender.Messaging.Response.GetStatus>(new Messaging.Request.Reload());
				if (objResp != null && objResp.Status == ResponseCode.Success) {
					UpdateStatus();
				} else {
					MessageBox.Show("Failed reloading item, please try again or contact Nico the almighty");
				}
				EndLoading();
			});
		}

		private void Reload() {
			ThreadPool.QueueUserWorkItem((state) => {
				SetLoading("Connection ready, fetching status from server...");
				UpdateStatus();

				SetLoading("Subscribing to workspace update event");
				WebSocketMessaging.Response.ACK objSubscribeResp = null;
				do {
					objSubscribeResp = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Subscribe.WorkspaceUpdated());
					if (objSubscribeResp == null || objSubscribeResp.Status != ResponseCode.Success) {
						if (objSubscribeResp.Status == ResponseCode.Disconnect) {
							return;
						}
						Thread.Sleep(2000);
					}
				} while (objSubscribeResp != null && objSubscribeResp.Status != ResponseCode.Success);
				EndLoading();
			});
		}

		private void mnuStart_Click(object sender, RoutedEventArgs e) {
			var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
			ThreadPool.QueueUserWorkItem((s) => {
				if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Processable || objWorkspaceItemViewModel.Status == Status.Paused) {
					objWorkspaceItemViewModel.IsUpdating = true;
					objWorkspaceItemViewModel.SelectedForHandling = true;

					var objResp = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Request.JobStart(objWorkspaceItemViewModel.ID));
					//deselect when we were able to queue the job
					if (objResp.Status == ResponseCode.Success) {
						objWorkspaceItemViewModel.SelectedForHandling = false;
					} else {
						MessageBox.Show("Failed starting item, please try again or contact Nico the almighty");
					}

					objWorkspaceItemViewModel.IsUpdating = false;
				}
			});
		}

		private void mnuStop_Click(object sender, RoutedEventArgs e) {
			var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
			ThreadPool.QueueUserWorkItem((s) => {
				if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Busy) {
					objWorkspaceItemViewModel.IsUpdating = true;
					objWorkspaceItemViewModel.SelectedForHandling = false;

					var objResp = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Request.JobStop(objWorkspaceItemViewModel.ID));
					if (objResp.Status == ResponseCode.Success) {
						objWorkspaceItemViewModel.SelectedForHandling = true;
					} else {
						MessageBox.Show("Failed stopping item, please try again or contact Nico the almighty");
					}
					objWorkspaceItemViewModel.IsUpdating = false;
				}
			});
		}

		private void mnuPause_Click(object sender, RoutedEventArgs e) {
			var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
			ThreadPool.QueueUserWorkItem((s) => {
				if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Busy) {
					objWorkspaceItemViewModel.IsUpdating = true;

					var objResp = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Request.JobPause(objWorkspaceItemViewModel.ID));
					if (objResp.Status != ResponseCode.Success) {
						MessageBox.Show("Failed pausing item, please try again or contact Nico the almighty");
					}
					objWorkspaceItemViewModel.IsUpdating = false;
				}
			});
		}

		private void mnuEditTargetName_Click(object sender, RoutedEventArgs e) {
			var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
			objWorkspaceItemViewModel.TargetNameIsEditing = true;
		}

		private void mnuEditSourceName_Click(object sender, RoutedEventArgs e) {
			var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
			objWorkspaceItemViewModel.SourceNameIsEditing = true;
		}

		private void ProjectField_CreateProjectClicked(object sender, RoutedEventArgs e) {
			try {
				if (!File.Exists(Settings.ShotcutExecutable)) { throw new Exception("Shotcut executable not found"); }

				var objWorkspaceItemViewModel = (sender as System.Windows.Documents.Hyperlink).DataContext as WorkspaceItemViewModel;
				ThreadPool.QueueUserWorkItem((s) => {
					if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.ProjectMissing) {
						objWorkspaceItemViewModel.IsUpdating = true;
						try {
							FileInfo objFI = MeltConfig.CreateConfig(objWorkspaceItemViewModel.WorkspaceItem);
							new Process {
								StartInfo = new ProcessStartInfo(Settings.ShotcutExecutable, "\"" + objFI.FullName + "\"")
							}.Start();
						} catch(Exception ex) {
							Log.Error(ex);
						}
						objWorkspaceItemViewModel.IsUpdating = false;
					}
				});
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		private void mnuOpenShotcut_Click(object sender, RoutedEventArgs e) {
			var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
			string strPath = Path.Combine(Settings.ProjectPath, objWorkspaceItemViewModel.ProjectName);
			if (File.Exists(strPath) && File.Exists(Settings.ShotcutExecutable)) {
				new Process {
					StartInfo = new ProcessStartInfo(Settings.ShotcutExecutable, "\"" + strPath + "\"")
				}.Start();
			}
		}
	}
}