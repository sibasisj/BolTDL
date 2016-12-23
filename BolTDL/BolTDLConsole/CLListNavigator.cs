﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BolTDL;

namespace BolTDLConsole
{
    class CLListNavigator : IListNavigator
    {
        public int CurrentTaskIndex
        {
            get
            {
                return _currentTaskIndex;
            }

            set
            {
                _currentTaskIndex = value;
            }
        }

        public ToDoList list
        {
            get
            {
                return _list;
            }

            set
            {
                _list = value;
            }
        }

        private int _currentTabCount
        {
            get
            {
                return listTabs.Count;
            }
        }

		private bool ListIsPopulated
		{
			get
			{
				return list.Length > 0;	
			}

		}

		public string GetUsername => settings.username;

        public List<ToDoList> listTabs;

        private BolTDLConsoleSettings settings;
        private int _currentTaskIndex, _currentTab;
        private ToDoList _list;
        //private bool _cursorAtTitle = true;
        private enum NavState { InList, OpenTask, AddingTask, PendingDelete, RenamingTab };
        private NavState state;
		//private bool runningOnMono;

        public CLListNavigator(ToDoList todolist)
        {
            list = todolist;
            state = NavState.InList;
            listTabs = new List<ToDoList>();
            listTabs.Add(todolist);
            _currentTab = 0;

			//runningOnMono = Type.GetType ("Mono.Runtime") != null;
        }

        public CLListNavigator(List<ToDoList> todolists)
        {
            if(todolists.Count > 0)
                list = todolists[0];
            state = NavState.InList;
            listTabs = todolists;
            _currentTab = 0;
        }

        public void FindTask()
        {
            throw new NotImplementedException();
        }

        public void NextTask()
        {
            if ((CurrentTaskIndex + 1) < list.Length)
            {
                CurrentTaskIndex++;
            }
        }

        public void PrevoiusTask()
        {
            if (CurrentTaskIndex > 0)
            {
                CurrentTaskIndex--;
            }
        }

        public void NextTab()
        {
            _currentTab++;
            if (_currentTab >= _currentTabCount)
                _currentTab = 0;

            list = listTabs[_currentTab];
        }

		public void GotoNewTab()
		{
			_currentTab = _currentTabCount - 1;
			list = listTabs [_currentTab];
		}

        public void PrintList()
        {
            Clear();

            if(list.Length > 0)
            {
                WriteTabs();
                BolTask currentTask;
                for (int i = 0; i < list.Length; i++)
                {
                    currentTask = list.GetTaskAt(i);
                    if (i == CurrentTaskIndex)
                        Console.Write(">   ");
                    else
                        Console.Write("    ");
                    Console.WriteLine(currentTask.Title);
                }

            }
            else
            {
                WriteTabs();
                Console.Write("You have no tasks! Press (O) to create a new task");
            }

            Navigate();

        }

        /// <summary>
        /// Handles the navigation, pretty much the primary class. Needs to be split into smaller methods.
        /// </summary>
        private void Navigate()
        {
            ConsoleKey key = Console.ReadKey().Key;
            Clear();
            Save();

            ////Remove the typed character
            //Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            //Console.Write(" "); 

            ////Start Writing at the top of the console
            //Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop - list.Length);


            //Navigation for InList
            if(state == NavState.InList)
            {
                if (key == ConsoleKey.J)
                {
                    NextTask();
                    GoList();
                }
                else if (key == ConsoleKey.K)
                {
                    state = NavState.InList;
                    PrevoiusTask();
                    GoList();
                }
                else if (key == ConsoleKey.L && ListIsPopulated)
                {
                    OpenTask();
                    Navigate();
                }
                else if (key == ConsoleKey.O)
                {
                    NavAddTask();
                    GoList();
                }
                else if (key == ConsoleKey.Q)
                {
                    return;
                }
                else if (key == ConsoleKey.W)
                {
                    Save();
                    GoList();
                }
                else if (key == ConsoleKey.D && ListIsPopulated)
                {
                    state = NavState.PendingDelete;
                    PrintList();
                }
                else if (key == ConsoleKey.C && ListIsPopulated)
                {
                    string name = list.GetTaskAt(_currentTaskIndex).Title;
                    list.DeleteTaskAt(_currentTaskIndex);
                    NavAddTask(name);
                    GoList();
                }
                else if (key == ConsoleKey.N)
                {
                    listTabs.Add(new ToDoList("New unnamed tab"));
					GotoNewTab ();
					RenameTab ();
                    GoList();
                }
				else if (key == ConsoleKey.T && state != NavState.PendingDelete)
                {
					CurrentTaskIndex = 0;
					NextTab ();
					GoList ();
                }
				else if (key == ConsoleKey.R)
				{
					RenameTab ();
					GoList ();
				}
                else
                {
                    state = NavState.InList;
                    PrintList();
                }
            }
            //Navigation for when a task is open
            else if(state == NavState.OpenTask)
            {
                if(key == ConsoleKey.Q ||key == ConsoleKey.H)
                {
                    GoList();
                }
                else if (key == ConsoleKey.I || key == ConsoleKey.A)
                {
                    //Edit
                    Navigate();
                }
				else if (key == ConsoleKey.J || key == ConsoleKey.K)
				{
					//Do nothing
					OpenTask();
					Navigate ();
				}
                else
                {
                    GoList();
                }
            }
			//Navigation for when pending a delete
			else if(state == NavState.PendingDelete)
			{
				if(key == ConsoleKey.D && ListIsPopulated)
				{
					DeleteCurrentTask();
					GoList ();
				}
				if(key == ConsoleKey.T)
				{
					//Delete list
					//list.AddTask(new BolTask("Title", ""));
					//Console.WriteLine ("MEEEEEEEEEEMES");

					DataHandler.TryDeleteSave (list.Name);
					listTabs.RemoveAt (_currentTab);
					CurrentTaskIndex = 0;
					NextTab ();
					GoList ();
				}
				else
				{
					GoList ();
				}
			}
        }

        private void Save()
        {
			/*	OLD SAVE METHOD WITH SEVERAL FILES
			 * foreach (var list in listTabs) {
				DataHandler.Save (list);
			} */

			//New Save method, single file
			DataHandler.ListSave (listTabs);
        }

        private void GoList()
        {
            state = NavState.InList;
            PrintList();
        }

        private void OpenTask()
        {
			state = NavState.OpenTask;
            BolTask currentTask = list.GetTaskAt(CurrentTaskIndex);
            Console.WriteLine("Title");
            Console.WriteLine("    " + currentTask.Title);

            if(settings.useDescriptions)
            {
                Console.WriteLine("Description");
                Console.WriteLine("    " + currentTask.Description);
            }

        }

        private void Clear()
        {
			//Windows
			Console.SetCursorPosition(0, 0);

			for (int i = 0; i < Console.WindowHeight; i++)
			{
				Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
			}
			Console.SetCursorPosition(0, 0);
        }

        private void NavAddTask()
        {
            state = NavState.AddingTask;
            Clear();
            Console.Write("Enter new title: ");
            string t = Console.ReadLine();
            if (settings.useDescriptions)
            {
                Console.Write("Enter description (optional): ");
                string d = Console.ReadLine();
                if (t == "")
                    return;
                list.AddTask(new BolTask(t, d));

                return;
            }

            list.AddTask(new BolTask(t, ""));
            
        }

        private void NavAddTask(string oldTaskName)
        {

            state = NavState.AddingTask;
            Clear();
            Console.WriteLine("Enter new title for task: " + oldTaskName);
            string t = Console.ReadLine();
            if (settings.useDescriptions)
            {
                Console.Write("Enter description (optional): ");
                string d = Console.ReadLine();
                if (t == "")
                    return;
                list.AddTask(new BolTask(t, d));

                return;
            }

            list.AddTask(new BolTask(t, ""));
        }

		private void DeleteCurrentTask()
		{
			list.DeleteTaskAt (CurrentTaskIndex);
			CurrentTaskIndex = 0;
		}

		private void RenameTab()
		{
			state = NavState.RenamingTab;
			Clear ();

			Console.WriteLine ("Enter new name for tab " + list.Name);
			string newName = Console.ReadLine();
			if(newName == "")
				newName = "Unnamed tab";
			DataHandler.TryDeleteSave (list.Name);
			list.SetName (newName);
			Save ();
		}

        private void WriteTabs()
        {
            ConsoleColor oldForeground = Console.ForegroundColor;
            ConsoleColor oldBackground = Console.BackgroundColor;

            for (int i = 0; i < _currentTabCount; i++)
            {
                if(i == _currentTab)
                {
                    if(settings.useColors)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.Write("> ");
                    }

                    Console.Write(listTabs[i].Name + "   ");

                    if(settings.useColors)
                    {
                        Console.ForegroundColor = oldForeground;
                        Console.BackgroundColor = oldBackground;
                    }
                }
                else
                {
                    Console.Write(listTabs[i].Name + "   ");
                }
            }
            Console.WriteLine("");
        }

        public void LoadSettings()
        {
            string json = DataHandler.ImportSettings(BolTDLConsoleSettings.fileName);

            settings = BolTDLConsoleSettings.SettingsFromJson(DataHandler.ImportSettings(BolTDLConsoleSettings.fileName));
			settings.ExportSettings();

            
        }
    }
}
