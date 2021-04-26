﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using GW2Scratch.ArcdpsLogManager.Logs;

namespace GW2Scratch.ArcdpsLogManager
{
	/// <summary>
	/// Lists files before deleting them, the User can remove Files from the List as well before confirming.
	/// </summary>
	public class DeleteFilesForm : Form
	{
		private FilterCollection<LogData> dataStore;

		private readonly Button confirmDeleteButton = new Button() { Text = "Delete files" };
		private readonly Button closeWindowButton = new Button() { Text = "Close" };
		private readonly Button removeSelectedButton = new Button() { Text = "Remove selected from list" };
		private readonly GridView logGrid = new GridView();

		public DeleteFilesForm(IEnumerable<LogData> files)
		{
			dataStore = new FilterCollection<LogData>(files);
			var layout = new DynamicLayout();

			Icon = Resources.GetProgramIcon();
			Title = "Confirm Deletion - arcdps Log Manager";
			ClientSize = new Size(500, 300);
			Resizable = true;
			Content = layout;

			var fileNameColumn = new GridColumn()
			{
				HeaderText = "Files",
				DataCell = new TextBoxCell
				{
					Binding = new DelegateBinding<LogData, string>(data => data.FileInfo.Name)
				}
			};

			var bossNameColumn = new GridColumn()
			{
				HeaderText = "Boss",
				DataCell = new TextBoxCell
				{
					Binding = new DelegateBinding<LogData, string>(data => data.Encounter.ToString())
				}
			};

			var encounterModeColumn = new GridColumn()
			{
				HeaderText = "Mode",
				DataCell = new TextBoxCell
				{
					Binding = new DelegateBinding<LogData, string>(data => data.EncounterMode.ToString())
				}
			};

			var encounterResultColumn = new GridColumn()
			{
				HeaderText = "Result",
				DataCell = new TextBoxCell
				{
					Binding = new DelegateBinding<LogData, string>(data => data.EncounterResult.ToString())
				}
			};

			logGrid.DataStore = dataStore;
			logGrid.AllowMultipleSelection = true;
			logGrid.Columns.Add(fileNameColumn);
			logGrid.Columns.Add(bossNameColumn);
			logGrid.Columns.Add(encounterModeColumn);
			logGrid.Columns.Add(encounterResultColumn);

			closeWindowButton.Click += (sender, args) => Close();
			confirmDeleteButton.Click += ConfirmDeleteButtonClicked;
			removeSelectedButton.Click += RemoveSelectedButtonClicked;

			layout.BeginGroup("Files Locations", new Padding(5), new Size(0, 5));
			{
				layout.Add(logGrid, yscale: true);
				layout.Add(null, yscale: false);
				layout.BeginVertical();
				{
					layout.BeginHorizontal();
					{
						layout.Add(removeSelectedButton, xscale: true);
						layout.Add(confirmDeleteButton, xscale: true);
						layout.Add(closeWindowButton, xscale: true);
					}
					layout.EndHorizontal();
				}
				layout.EndVertical();
			}
			layout.EndGroup();
			Show();
		}

		private void RemoveSelectedButtonClicked(object sender, EventArgs e)
		{
			dataStore = new FilterCollection<LogData>(dataStore.Where(log => !logGrid.SelectedItems.Contains(log)));
			logGrid.DataStore = dataStore;
		}

		private void ConfirmDeleteButtonClicked(object sender, EventArgs e)
		{
			//Call the delete function on the 
			DeleteFiles(dataStore.Select(log => log.FileName));
			Close();
		}

		private void DeleteFiles(IEnumerable<string> filesToDelete)
		{
			foreach (var file in filesToDelete)
			{
				// File.Delete doesnt throw an Error if the file doesnt exist
				//File.Delete(file);
			}
		}
	}
}