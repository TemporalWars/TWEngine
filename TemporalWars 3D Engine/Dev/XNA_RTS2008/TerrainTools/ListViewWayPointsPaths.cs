using System.Drawing;
using System.Windows.Forms;
using System;
using System.Diagnostics;

namespace TWEngine.TerrainTools
{
    // 10/15/2009
    /// <summary>
    /// ListView extension to show a red-line for the re-ordering of items in the ListView; Code comes from
    /// http://www.codeproject.com/KB/list/LVCustomReordering.aspx.
    /// </summary>
    class ListViewWayPointsPaths : ListView
    {
       // from WinUser.h
// ReSharper disable InconsistentNaming
        private const int WM_PAINT = 0x000F;
// ReSharper restore InconsistentNaming

        public ListViewWayPointsPaths()
        {
            try // 6/22/2010
            {
                // Reduce flicker
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ListViewWayPointsPaths method threw the exception " + ex.Message ?? "No Message"); 
#endif
            }
        }

        private int _lineBefore = -1;
        /// <summary>
        /// If set to a value >= 0, an insertion line is painted before the item with the given index.
        /// </summary>
        public int LineBefore
        {
            get { return _lineBefore; }
            set { _lineBefore = value; }
        }

        private int _lineAfter = -1;
        /// <summary>
        /// If set to a value >= 0, an insertion line is painted after the item with the given index.
        /// </summary>
        public int LineAfter
        {
            get { return _lineAfter; }
            set { _lineAfter = value; }
        }

        protected override void WndProc(ref Message m)
        {
            try // 6/22/2010
            {
                base.WndProc(ref m);

                // We have to take this way (instead of overriding OnPaint()) because the ListView is just a wrapper
                // around the common control ListView and unfortunately does not call the OnPaint overrides.
                if (m.Msg != WM_PAINT) return;

                if (LineBefore >= 0 && LineBefore < Items.Count)
                {
                    Rectangle rc = Items[LineBefore].GetBounds(ItemBoundsPortion.Entire);
                    DrawInsertionLine(rc.Left, rc.Right, rc.Top);
                }
                if (LineAfter >= 0 && LineBefore < Items.Count)
                {
                    Rectangle rc = Items[LineAfter].GetBounds(ItemBoundsPortion.Entire);
                    DrawInsertionLine(rc.Left, rc.Right, rc.Bottom);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("WndProc method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        /// <summary>
        /// Draw a line with insertion marks at each end
        /// </summary>
        /// <param name="x1">Starting position (X) of the line</param>
        /// <param name="x2">Ending position (X) of the line</param>
        /// <param name="y">Position (Y) of the line</param>
        private void DrawInsertionLine(int x1, int x2, int y)
        {
            try // 6/22/2010
            {
                using (Graphics g = CreateGraphics())
                {
                    g.DrawLine(Pens.Red, x1, y, x2 - 1, y);

                    var leftTriangle = new[] {
                                new Point(x1,      y-4),
                                new Point(x1 + 7,  y),
                                new Point(x1,      y+4)
                            };
                    var rightTriangle = new[] {
                                new Point(x2,     y-4),
                                new Point(x2 - 8, y),
                                new Point(x2,     y+4)
                            };
                    g.FillPolygon(Brushes.Red, leftTriangle);
                    g.FillPolygon(Brushes.Red, rightTriangle);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("DrawInsertionLine method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }
    }
}
