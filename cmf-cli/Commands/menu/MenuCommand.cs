using Cmf.CLI.Commands.Bump;
using Cmf.CLI.Commands.DbManager;
using Cmf.CLI.Commands.Generate;
using Cmf.CLI.Commands.Install;
using Cmf.CLI.Commands.Run;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///     MenuCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("menu", Id = "menu")]
    public class MenuCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.AddOption(
                new Option<long>(
                    aliases: new[] { "--licenseId" },
                    description: "Project License Id"
                )
                { IsRequired = true }
            );

            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        /// <param name="licenseId">
        ///     Project License Id
        /// </param>
        public void Execute(long licenseId)
        {
            GenericUtilities.EnsureIsRunningAsAdmin();

            Menu menu = new Menu(
                prompt: "Select what to run:",
                items: new List<MenuItem>
                {
                    new MenuItem(
                        identifier: 1,
                        title: "Install All",
                        action: () => new InstallCommand().Execute(licenseId),
                        children: new List<MenuItem>
                        {
                            new MenuItem(
                                identifier: 11,
                                title: "Install Business",
                                action: () => new InstallBusinessCommand().Execute(licenseId)
                            ),
                            new MenuItem(
                                identifier: 12,
                                title: "Install HTML",
                                action: () => new InstallHtmlCommand().Execute()
                            ),
                            new MenuItem(
                                identifier: 13,
                                title: "Install Help",
                                action: () => new InstallHelpCommand().Execute()
                            ),
                            new MenuItem(
                                identifier: 14,
                                title: "Install IoT",
                                action: () => new InstallIotCommand().Execute()
                            )
                        }
                    ),
                    new MenuItem(
                        identifier: 2,
                        title: "Run All (MessageBus | Host | HTML)",
                        action: () => new RunCommand().Execute(),
                        children: new List<MenuItem>
                        {
                            new MenuItem(
                                identifier: 21,
                                title: "Run MessageBus",
                                action: () => new RunMessageBusCommand().Execute()
                            ),
                            new MenuItem(
                                identifier: 22,
                                title: "Run Host",
                                action: () => new RunHostCommand().Execute()
                            ),
                            new MenuItem(
                                identifier: 23,
                                title: "Run HTML",
                                action: () => new RunHTMLCommand().Execute()
                            ),
                            new MenuItem(
                                identifier: 24,
                                title: "Run Help",
                                action: () => new RunHelpCommand().Execute()
                            )
                        }
                    ),
                    new MenuItem(
                        identifier: 3,
                        title: "Local DB Manager",
                        children: new List<MenuItem>
                        {
                            new MenuItem(
                                identifier: 31,
                                title: "Backup Local DB",
                                action: () => new BackupDatabaseCommand().Execute()
                            ),
                            new MenuItem(
                                identifier: 32,
                                title: "Restore Local DB",
                                action: () => new RestoreDatabaseCommand().Execute()
                            ),
                            new MenuItem(
                                identifier: 33,
                                title: "Delete Local DB Backup File",
                                action: () => new DeleteDatabaseBackupCommand().Execute()
                            )
                        },
                        disabled: true
                    ),
                    new MenuItem(
                        identifier: 4,
                        title: "Generate LBOs",
                        action: () => new GenerateLBOsCommand().Execute()
                    ),
                    new MenuItem(
                        identifier: 5,
                        title: "Generate Documentation",
                        action: () => new GenerateDocumentationCommand().Execute()
                    ),
                    new MenuItem(
                        identifier: 6,
                        title: "Generate Release Notes (BROKEN)",
                        action: () => new GenerateReleaseNotesCommand().Execute()
                    ),
                    new MenuItem(
                        identifier: 7,
                        title: "Bump Version (Interactive)",
                        action: () => new BumpInteractiveCommand().Execute()
                    ),
                }
            );

            Log.Information($"---------- {ExecutionContext.Instance.ProjectConfig.Tenant} ----------");
            Log.Verbose($"Current License: {licenseId}");
            Log.Verbose($"Current Database: {GenericUtilities.GetCurrentDb(fileSystem)}");
            Log.Verbose("");
            menu.Run();
        }
    }

    /// <summary>
    ///     Represents a Menu with a Prompt and a List of <see cref="MenuCommand"/>.
    /// </summary>
    public class Menu
    {
        /// <summary>
        ///     Gets or sets the prompt.
        /// </summary>
        /// <value>
        ///     Text written in console before printing the Menu Items
        /// </value>
        public string Prompt { get; set; }

        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        /// <value>
        ///     List of Menu Items
        /// </value>
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Menu"/> class with an empty List of <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="prompt">
        ///     See <see cref="Prompt"/>
        /// </param>
        public Menu(string prompt)
        {
            Prompt = prompt;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Menu"/> class with given List of <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="prompt">
        ///     See <see cref="Prompt"/>
        /// </param>
        /// <param name="items">
        ///     See <see cref="Items"/>
        /// </param>
        public Menu(string prompt, List<MenuItem> items) : this(prompt)
        {
            Items = items;
        }

        #endregion Constructors

        #region Private Methods/Functions

        /// <summary>
        ///     Searches for a <see cref="MenuItem"/> with a specific identifier, including nested levels (Children of Children).
        /// </summary>
        /// <param name="identifier">
        ///     Identifier of the <see cref="MenuItem"/> to find.
        /// </param>
        /// <returns>
        ///     <para>Returns a <see cref="MenuItem"/> that matches the provided identifier and is not disabled.</para>
        ///     <para>
        ///         If a matching item is found in the first level of children, it is returned immediately. If not, the method searches in the Items Children
        ///         and Children of Children (2nd and subsequent levels) until a matching item is found, and that item is returned. If no matching item is
        ///         found, a null value is returned.
        ///     </para>
        /// </returns>
        private MenuItem FindItem(int identifier)
        {
            // Find in Menu.Items (1st level)
            MenuItem item = Items.Find(item => item.Identifier == identifier && !item.Disabled);
            if (!item.IsNullOrEmpty()) return item;

            foreach (MenuItem menuItem in Items)
            {
                // Find in Menu.Items.Children recursively (2nd and subsequent levels)
                item = menuItem.FindChildren(identifier);
                if (!item.IsNullOrEmpty()) break;
            }

            return item;
        }

        /// <summary>
        ///     Reads an option from the console until a valid <see cref="MenuItem"/> is found.
        /// </summary>
        /// <returns>
        ///     <see cref="MenuItem"/>
        /// </returns>
        private MenuItem ReadOption()
        {
            MenuItem item = null;

            while (item.IsNullOrEmpty())
            {
                int option = GenericUtilities.ReadIntValueFromConsole(prompt: "Option:");
                item = FindItem(option);
                if (item.IsNullOrEmpty())
                {
                    Log.Error("Invalid option. Try again.");
                }
            }

            return item;
        }

        #endregion Private Methods/Functions

        #region Public Methods/Functions

        /// <summary>
        ///     Runs the Menu
        /// </summary>
        public void Run()
        {
            // Print the Menu
            Log.Verbose(Prompt);
            foreach (MenuItem item in Items)
            {
                item.Print();
            }

            // Read and execute the MenuItem.Action
            ReadOption().Action();
        }

        #endregion Public Methods/Functions
    }

    /// <summary>
    ///     Represents a MenuItem that can execute <see cref="Action"/> and have Children <see cref="MenuItem"/>.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     Number that the user will input to execute this <see cref="MenuItem"/>
        /// </value>
        public int Identifier { get; set; }

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        /// <value>
        ///     Text that will be written next to the <see cref="Identifier"/>.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether disabled.
        /// </summary>
        /// <value>
        ///     Indicates if the <see cref="MenuItem"/> is disabled or not. Disabled means that no <see cref="Identifier"/> will be displayed.
        /// </value>
        public bool Disabled { get; set; }

        /// <summary>
        ///     Gets or sets the action.
        /// </summary>
        /// <value>
        ///     Action that will be executed when the user chooses an option from the menu
        /// </value>
        public Action Action { get; set; }

        /// <summary>
        ///     Gets or sets the children.
        /// </summary>
        /// <value>
        ///     List of <see cref="MenuItem"/> that are children
        /// </value>
        public List<MenuItem> Children { get; set; } = new List<MenuItem>();

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="identifier">
        ///     The identifier.
        /// </param>
        /// <param name="title">
        ///     The title.
        /// </param>
        public MenuItem(int identifier, string title)
        {
            Identifier = identifier;
            Title = title;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="identifier">
        ///     The identifier.
        /// </param>
        /// <param name="title">
        ///     The title.
        /// </param>
        /// <param name="children">
        ///     The children.
        /// </param>
        /// <param name="disabled">
        ///     If true, disabled.
        /// </param>
        public MenuItem(int identifier, string title, List<MenuItem> children, bool disabled = false) : this(identifier, title)
        {
            Children = children;
            Disabled = disabled;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="identifier">
        ///     The identifier.
        /// </param>
        /// <param name="title">
        ///     The title.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        public MenuItem(int identifier, string title, Action action) : this(identifier, title)
        {
            Action = action;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="identifier">
        ///     The identifier.
        /// </param>
        /// <param name="title">
        ///     The title.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        /// <param name="children">
        ///     The children.
        /// </param>
        public MenuItem(int identifier, string title, Action action, List<MenuItem> children) : this(identifier, title, children)
        {
            Action = action;
        }

        #endregion Constructors

        #region Private Methods/Functions

        /// <summary>
        ///     Prints the Menu Item
        /// </summary>
        /// <param name="currentRecursionLevel">
        ///     Current recursion level. This will add spaces when printing.
        /// </param>
        private void PrintToConsole(int currentRecursionLevel = 0)
        {
            // Calculate the indentation based on the current recursion level.
            string indentation = string.Empty;
            for (int i = 0; i < currentRecursionLevel; i++)
            {
                indentation += "  ";
            }

            Log.Verbose($"{indentation}{(Disabled ? "X" : Identifier)} - {Title}");
        }

        #endregion Private Methods/Functions

        #region Public Methods/Functions

        /// <summary>
        ///     Recursively Prints the Menu Item and its Children
        /// </summary>
        /// <param name="level">
        ///     Current Recursion level. Always starts counting from 0.
        /// </param>
        public void Print(int level = 0)
        {
            PrintToConsole(level);
            foreach (MenuItem child in Children)
            {
                child.Print(level + 1);
            }
        }

        /// <summary>
        ///     Recursively searches for a <see cref="MenuItem"/> with a specific identifier within the Children collection.
        /// </summary>
        /// <param name="identifier">
        ///     Identifier of the <see cref="MenuItem"/> to find.
        /// </param>
        /// <returns>
        ///     <para>Returns a <see cref="MenuItem"/> that matches the provided identifier and is not disabled.</para>
        ///     <para>
        ///         If a matching item is found in the first level of children, it is returned immediately. If not, the method recursively searches through the
        ///         Children of Children (2nd and subsequent levels) until a matching item is found, and that item is returned. If no matching item is found, a
        ///         null value is returned.
        ///     </para>
        /// </returns>
        public MenuItem FindChildren(int identifier)
        {
            // Find in MenuItem.Children (1st level)
            MenuItem item = Children.Find(item => item.Identifier == identifier && !item.Disabled);
            if (!item.IsNullOrEmpty()) return item;

            foreach (MenuItem child in Children)
            {
                // Find in MenuItem.Children.Children recursively (2nd and subsequent levels)
                item = child.FindChildren(identifier);
                if (!item.IsNullOrEmpty()) break;
            }

            return item;
        }

        #endregion Public Methods/Functions
    }
}