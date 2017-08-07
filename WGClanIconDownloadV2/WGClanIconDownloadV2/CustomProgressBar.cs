/// https://stackoverflow.com/questions/3529928/how-do-i-put-text-on-progressbar
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CustomProgressBar
{
    public enum ProgressBarDisplayText
    {
        Percentage,
        CustomText
    }

    public class ProgressBarWithCaption : ProgressBar
    {
        //Property to set to decide whether to print a % or Text
        public ProgressBarDisplayText DisplayStyle { get; set; }

        //Property to hold the custom text
        public string CustomText { get; set; }

        public ProgressBarWithCaption()
        {
            // Modify the ControlStyles flags
            //http://msdn.microsoft.com/en-us/library/system.windows.forms.controlstyles.aspx
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;

            ProgressBarRenderer.DrawHorizontalBar(g, rect);
            rect.Inflate(-3, -3);
            if (Value > 0)
            {
                // As we doing this ourselves we need to draw the chunks on the progress bar
                Rectangle clip = new Rectangle(rect.X, rect.Y, (int)Math.Round(((float)Value / Maximum) * rect.Width), rect.Height);
                ProgressBarRenderer.DrawHorizontalChunks(g, clip);
            }

            // Set the Display text (Either a % amount or our custom text
            string text = DisplayStyle == ProgressBarDisplayText.Percentage ? Value.ToString() + '%' : CustomText;

            using (Font f = new Font(FontFamily.GenericSansSerif, 10))
            {
                SizeF len = g.MeasureString(text, f);
                // Calculate the location of the text (the middle of progress bar)
                // Point location = new Point(Convert.ToInt32((rect.Width / 2) - (len.Width / 2)), Convert.ToInt32((rect.Height / 2) - (len.Height / 2)));
                Point location = new Point(Convert.ToInt32((Width / 2) - len.Width / 2), Convert.ToInt32((Height / 2) - len.Height / 2));
                // The commented-out code will centre the text into the highlighted area only. This will centre the text regardless of the highlighted area.
                // Draw the custom text
                g.DrawString(text, f, Brushes.Black, location);
            }
        }
    }

    public class ProgressBarWithCaptionVista : ProgressBar
    {
        //Property to set to decide whether to print a % or Text
        private ProgressBarDisplayText m_DisplayStyle;
        public ProgressBarDisplayText DisplayStyle
        {
            get { return m_DisplayStyle; }
            set { m_DisplayStyle = value; }
        }

        //Property to hold the custom text
        private string m_CustomText;
        public string CustomText
        {
            get { return m_CustomText; }
            set
            {
                m_CustomText = value;
                this.Invalidate();
            }
        }

        private const int WM_PAINT = 0x000F;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case WM_PAINT:
                    int m_Percent = Convert.ToInt32((Convert.ToDouble(Value) / Convert.ToDouble(Maximum)) * 100);
                    dynamic flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.WordEllipsis;

                    using (Graphics g = Graphics.FromHwnd(Handle))
                    {
                        using (Brush textBrush = new SolidBrush(ForeColor))
                        {

                            switch (DisplayStyle)
                            {
                                case ProgressBarDisplayText.CustomText:
                                    TextRenderer.DrawText(g, CustomText, new Font(FontFamily.GenericSansSerif, Convert.ToSingle(10), FontStyle.Regular), new Rectangle(0, 0, this.Width, this.Height), Color.Black, flags); 
                                    // TextRenderer.DrawText(g, CustomText, new Font("Arial", Convert.ToSingle(8.25), FontStyle.Regular), new Rectangle(0, 0, this.Width, this.Height), Color.Black, flags);
                                    break;
                                case ProgressBarDisplayText.Percentage:
                                    TextRenderer.DrawText(g, string.Format("{0}%", m_Percent), new Font(FontFamily.GenericSansSerif, Convert.ToSingle(10), FontStyle.Regular), new Rectangle(0, 0, this.Width, this.Height), Color.Black, flags);
                                    // TextRenderer.DrawText(g, string.Format("{0}%", m_Percent), new Font("Arial", Convert.ToSingle(8.25), FontStyle.Regular), new Rectangle(0, 0, this.Width, this.Height), Color.Black, flags);
                                    break;
                            }
                        }
                    }
                    break;
            }

        }

    }
}