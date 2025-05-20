using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformHelpers
{
    public static class Create
    {
        public static Label NewLabel(
            string labelText,
            int posX, int posY,
            int width, int height,
            Color color)
        {
            Label label = new();
            label.Text = labelText;
            label.Location = new Point(posX, posY);
            label.Size = new Size(width, height);
            label.ForeColor = color;
            return label;
        }

        public static Label NewLabel(
            string labelText,
            int posX, int posY,
            int width, int height)
        {
            return NewLabel(labelText, posX, posY, width, height, Color.Black);
        }

        public static Button NewButton(
            string buttonText,
            int posX, int posY,
            int width, int height)
        {
            Button button = new();
            button.Text = buttonText;
            button.Location = new Point(posX, posY);
            button.Size = new Size(width, height);
            return button; 
        }

        public static TextBox NewTextBox(
            string defaultText,
            int posX, int posY,
            int width, int height)
        {
            TextBox box = new();
            box.Text = defaultText;
            box.Location = new Point(posX, posY);
            box.Size = new Size(width, height);
            return box;
        }

        public static CheckBox NewCheckBox(
            int posX, int posY,
            int width, int height,
            bool checkedAtStart)
        {
            CheckBox checkBox = new();
            checkBox.Location = new Point(posX, posY);
            checkBox.Size = new Size(width, height);
            checkBox.Checked = checkedAtStart;
            return checkBox;
        }


    }
}
