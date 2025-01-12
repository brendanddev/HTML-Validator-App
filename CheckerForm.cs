/*
    Class: CheckerForm.cs
    Author: Brendan Dileo
    Date: November 16, 2024
    
    Purpose: Provides the functionality behind the form application for Assignment 4: 'Part B - A Tangled Web'.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HTMLCheckerForm
{
    /// <summary>
    /// @author: Brendan Dileo
    /// </summary>
    public partial class CheckerForm : Form
    {
        /// <summary>
        /// A list of strings that will be used to store each of the html tags read from a file.
        /// </summary>
        private List<string> tags = new List<string>();

        /// <summary>
        /// A stack of strings that will be used to determine if the html tags are balanced.
        /// </summary>
        private Stack<string> stack = new Stack<string>();

        /// <summary>
        /// A string storing the corresponding files file path. Declared as class level for access in all methods.
        /// </summary>
        string file;

        /// <summary>
        /// Initialize the form components.
        /// </summary>
        public CheckerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the event triggered by the load menu item being clicked.
        /// This indicates the user wanting to load a file into the application.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event data unused</param>
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReadData();
        }

        /// <summary>
        /// Handles the event triggered by the check tags menu item being clicked.
        /// This indicates the user wanting to check the tags of an html file.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event data unused</param>
        private void checkTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckTags();
        }

        /// <summary>
        /// Handles the event triggered by the exit menu item being clicked.
        /// Indicates the user wanting to exit the application.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event data unused</param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the event triggered by the user attempting to exit the form.
        /// A message box is displayed to the user asking them to confirm if they want to exit the application.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event data unused</param>
        private void CheckerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; // Cancels form close
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes) // Checks if user has selected 'yes', meaning user wants the form to close
            {
                e.Cancel = false; // Closes form
            }
        }

        /// <summary>
        /// Reads data from an html file.
        /// This method makes use of an instance of the open file dialog class to let the user select a file, with the filter property so the user can only
        /// select html files. An instance of the stream reader class is used to read the data from the file line by line, until the end of the file is reached.
        /// A regular expression is used to check for valid html tags, and add each matching tag into a match collection. This is used to store each of the 
        /// substrings that match the regular expression pattern. The collection of matching html tags is iterated through, and added to a list that contains
        /// all of the html tags in the file. Once this process has completed, the forms status label will be updated to reflect the file being loaded successfully.
        /// </summary>
        private void ReadData()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog(); // Used for file selection
                openFileDialog.Filter = "HTML files (*.html)|*.html"; // Specifies only html files can be selected

                if (openFileDialog.ShowDialog() == DialogResult.OK) // Checks if user chose a valid file and selected ok
                {
                    file = openFileDialog.FileName; // Stores the file path of the chosen file

                    using (StreamReader reader = new StreamReader(file)) // Uses stream reader to read data from specified file
                    {
                        tags.Clear(); // Clear any prior data before reading new data
                        
                        string line; // Stores each line read from the file
                        while ((line = reader.ReadLine()) != null) // Continues to read lines from the file until end of file is reached
                        {
                            Regex tagRegex = new Regex(@"<\s*(\/?[a-zA-Z0-9]+)\s*[^>]*>"); // Defines the regex pattern used to match the valid html tags
                            MatchCollection matches = tagRegex.Matches(line); // Stores all strings that match the regex pattern into a collection of matches by calling the matches method

                            foreach (Match match in matches) // Iterates through each match object in the collection of matches
                            {
                                string tag = match.Groups[1].Value.ToLower(); // Stores only the name of the html tag into a string
                                tags.Add(tag);
                            }
                        }

                        checkTagsToolStripMenuItem.Enabled = true; // Enables check tags menu item now that file is selected and loaded
                        tagsLabel.ForeColor = Color.Green;
                        tagsLabel.Text = $"{Path.GetFileName(file)} loaded successfully! "; // Displays file name and success message
                    }
                }
            }
            catch (FileNotFoundException ex) // Cant find selected file
            {
                MessageBox.Show($"The file could not be found. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex) // Issue with reading file
            {
                MessageBox.Show($"An error occurred when reading from the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (Exception ex) // General exception handling for unexpected errors
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Determines whether an html file contains balanced tags or not.
        /// This method makes use of a stack, and a hard coded list of self closing tags, to determine whether or not the selected html file contains balanced tags.
        /// The list of the parsed tags is looped through, and first checked to see if it is self closing. If it is, it is not pushed to the stack. Otherwise the tag
        /// is checked to see if it is opening or closing. If an open tag is encountered, it is pushed to the stack. If a closing tag is encountered, it is popped from 
        /// the stack. Once each of the tags have been looped through, the stack is checked to see if it contains any elements. If it does, this indicates an imbalance
        /// in the tags. If the stacks count is 0, the tags are balanced. Depending on which of these statements is true, the forms status label will be updated to 
        /// reflect it.
        /// </summary>
        private void CheckTags()
        {
            try
            {
                // Hard coded list of common self closing (non-container) html tags
                List<string> selfClosingTags = new List<string>() { "br", "hr", "img", "input", "meta", "link", "area", "source", "track", "base", "col", "embed", "wbr" };
                int indentCount = 0; // Controls the indentation before a tag

                if (tags == null || tags.Count == 0) // Checks if list of tags is null or empty
                {
                    tagsLabel.ForeColor = Color.Red;
                    tagsLabel.Text = "The file does not contain valid HTML!";
                    MessageBox.Show("Please provide valid HTML tags!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                stack.Clear(); // Clears the content from the stack

                tagsRichTextBox.Clear(); // Clears the content from the text box

                bool balanced = true; // Flag that tracks if the html tags are balanced or not

                foreach (string tag in tags) // Loops through each tag in the list of tags
                {
                    string indent = new string(' ', indentCount * 4); // Creates a string of whitespace acting as the indent determined by how nested an html tag is in the file

                    if (selfClosingTags.Contains(tag.ToLower())) // Checks if the current tag is one of the self closing tags
                    {
                        tagsRichTextBox.SelectionColor = Color.OrangeRed;
                        tagsRichTextBox.AppendText($"{indent}Found non-container tag: <{tag}>!\r\n");
                        continue; // Move to next tag
                    }

                    if (tag.StartsWith("/")) // Check if the current tag is a closing tag
                    {
                        string name = tag.Substring(1); // Starts new string at index 1 of the previous string, removing the leading slash and storing the tag name
                        
                        if (stack.Count > 0 && stack.Peek() == name) // Checks if the stack it not empty and if the name of the top item in the stack matches the name of the current tag
                        {
                            indentCount--; // Decrease indent since closing tag was found
                            indent = new string(' ', indentCount * 4); // Whitespace is now determined by updated indent count
                            tagsRichTextBox.SelectionColor = Color.DarkGreen;
                            tagsRichTextBox.AppendText($"{indent}Found closing tag: <{tag}>!\r\n");
                            stack.Pop(); // Removes the matching opening tag from the stack
                        }
                        else // Stack is empty or the closing tag does not match the most recent opening tag, indicating a imbalance in the html tags
                        {
                            indentCount -= 2; // Ensures indentation format stays consistent even after imbalance in tags found
                            indent = new string(' ', indentCount * 4); // Whitespace for indent determined by updated indent count
                            tagsRichTextBox.AppendText($"{indent}Found closing tag: <{tag}>!\r\n");
                            tagsLabel.ForeColor = Color.Red;
                            tagsLabel.Text = $"Tags are not balanced in {Path.GetFileName(file)}!";
                            return;
                        }
                    }
                    else // Not a closing tag, means its an opening tag
                    {
                        tagsRichTextBox.SelectionColor = Color.DarkBlue;
                        tagsRichTextBox.AppendText($"{indent}Found opening tag: <{tag}>!\r\n");
                        stack.Push(tag); // Pushes the opening tag onto the stack to then be used to check for a matching closing tag
                        indentCount++; // Increase indent since opening tag was found
                    }
                }

                if (stack.Count > 0) // Checks if the stack still contains tags after the loop
                {
                    balanced = false; // Stack isnt empty so the tags are not balanced
                    while (stack.Count > 0) // Iterates through the remaining tags in the stack
                    {
                        string unclosedTag = stack.Pop(); // Removes the element at the top of the stack and stores it into a string
                        tagsRichTextBox.SelectionColor = Color.Red;
                        tagsRichTextBox.AppendText($"Unclosed tag: <{unclosedTag}>!\r\n"); // Not really used, could be implemented but wanted to follow the example assignment
                    }

                    tagsLabel.ForeColor = Color.Red;
                    tagsLabel.Text = $"Tags are not balanced in {Path.GetFileName(file)}!"; // Indicates tags are not balanced
                }
                else if (balanced)
                {
                    tagsLabel.Text = $"All tags are balanced {Path.GetFileName(file)}!"; // Indicates tags are balanced
                }
            }
            catch (Exception ex) // General error handling for unexpected errors
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}