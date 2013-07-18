using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;
using ICSharpCode.AvalonEdit.Folding;

namespace LFStudio
{
    class XBackgroundRenderer : IBackgroundRenderer
    {
        TextEditor editor;
        LinearGradientBrush lgbLine = new LinearGradientBrush();
        Pen border = new Pen();
        Pen lpen = new Pen();
        SolidColorBrush scbOdd = new SolidColorBrush();
        SolidColorBrush scbEven = new SolidColorBrush();

      /*  public bool isEnabledActiveLineHighlight;
        public bool isEnabledBackgroundHighlight;
        public string oddLineColor;
        public string evenLineColor;
        public string firstActiveLineColor;
        public string secondActiveLineColor;
        public string isFixedBackgroundLines;*/
        public XBackgroundRenderer(TextEditor e)
        {
            editor = e;
            lgbLine.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(G.AppSettings.firstActiveLineColor), 0));
            lgbLine.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(G.AppSettings.secondActiveLineColor), 1));
            scbOdd.Color = (Color)ColorConverter.ConvertFromString(G.AppSettings.oddLineColor);
            scbEven.Color = (Color)ColorConverter.ConvertFromString(G.AppSettings.evenLineColor);
            SolidColorBrush scb = new SolidColorBrush(Colors.Black);
            lpen = new Pen(scb, 1);
            lpen.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);            

        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Background; }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
          
            try
            {
                textView.EnsureVisualLines();
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                bool odd = true;
                if (!G.AppSettings.isFixedBackgroundLines)
                    if (textView.VisualLines.Count>0)
                    if (textView.VisualLines[0].FirstDocumentLine.LineNumber % 2 == 0)
                        odd = false;
                for (int i = 0; i < textView.VisualLines.Count; i++)
                {
                    var segment2 = new TextSegment
                    {
                        StartOffset = textView.VisualLines[i].FirstDocumentLine.Offset,
                        EndOffset = textView.VisualLines[i].LastDocumentLine.EndOffset
                    };
                    foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment2))
                    {
                        if (!odd)
                            drawingContext.DrawRectangle(scbEven, border,
                                                           new Rect(r.Location, new Size(textView.ActualWidth, r.Height)));//draw even line
                        else drawingContext.DrawRectangle(scbOdd, border,
                                                                new Rect(r.Location, new Size(textView.ActualWidth, r.Height)));//draw odd line
                        if (G.AppSettings.isShowUnderline)
                        {
                            string str = editor.Document.GetText(textView.VisualLines[i].FirstDocumentLine.Offset, textView.VisualLines[i].FirstDocumentLine.Length);
                            //if (str.Contains("<frame_end>") || str.Contains("<bmp_end>"))
                            if (Contains(str, G.AppSettings.underlineThisWords))
                            {
                                double halfPenWidth = lpen.Thickness / 2;
                                GuidelineSet guidelines = new GuidelineSet();
                                guidelines.GuidelinesX.Add(r.Left + halfPenWidth);
                                guidelines.GuidelinesX.Add(r.Right + halfPenWidth);
                                guidelines.GuidelinesY.Add(r.Top + halfPenWidth);
                                guidelines.GuidelinesY.Add(r.Bottom + halfPenWidth);
                                drawingContext.PushGuidelineSet(guidelines);
                                drawingContext.DrawLine(lpen, new Point(r.Left, r.Bottom), new Point(textView.ActualWidth, r.Bottom)); //draw underline                            
                            }
                        }
                        if (odd) odd = false; else odd = true;
                    }     //foreach
                }    //for
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                var segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
                {
                    drawingContext.DrawRectangle(lgbLine, border,
                                                 new Rect(r.Location, new Size(textView.ActualWidth, r.Height))); //Draw current cursor line
                    if (G.AppSettings.isShowUnderline)
                    {
                        string str2 = editor.Document.GetText(segment);

                        if (Contains(str2, G.AppSettings.underlineThisWords))
                        {
                            double halfPenWidth = lpen.Thickness / 2;
                            GuidelineSet guidelines = new GuidelineSet();
                            guidelines.GuidelinesX.Add(r.Left + halfPenWidth);
                            guidelines.GuidelinesX.Add(r.Right + halfPenWidth);
                            guidelines.GuidelinesY.Add(r.Top + halfPenWidth);
                            guidelines.GuidelinesY.Add(r.Bottom + halfPenWidth);
                            drawingContext.PushGuidelineSet(guidelines);
                            drawingContext.DrawLine(lpen, new Point(r.Left, r.Bottom), new Point(textView.ActualWidth, r.Bottom)); //correct underline
                        }
                    }
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }//Draw end
        public bool Contains(string where, List<string> what)
        {
            foreach (string item in what)
                if (where.Contains(item)) return true;
            return false;
        }
    }
}