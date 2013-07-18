using System;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace LFStudio
{
    class LinesBackgroundRenderer : IBackgroundRenderer
    {
        TextEditor editor;     
        SolidColorBrush scbOdd = new SolidColorBrush();
        SolidColorBrush scbEven = new SolidColorBrush();     
        Pen border = new Pen();
        public LinesBackgroundRenderer(TextEditor e)
        {                      
            editor = e;
            scbOdd.Color =(Color)ColorConverter.ConvertFromString(G.AppSettings.oddLineColor);               
            scbEven.Color = (Color)ColorConverter.ConvertFromString(G.AppSettings.evenLineColor);               
        }
        public KnownLayer Layer
        {
            get { return KnownLayer.Background; }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {

            textView.EnsureVisualLines();
            bool odd = true;
            if (!G.AppSettings.isFixedBackgroundLines)
            if (textView.VisualLines[0].FirstDocumentLine.LineNumber % 2 == 0)
               odd = false;            
            for (int i = 0; i < textView.VisualLines.Count; i++)
            {
                //textView.LineTransformers.Add(IVisualLineTransformer)
                //textView.Options
                var segment = new TextSegment
                {
                    StartOffset = textView.VisualLines[i].FirstDocumentLine.Offset,
                    EndOffset = textView.VisualLines[i].LastDocumentLine.EndOffset
                };
                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
                {

                    if (!odd)
                    //if (textView.VisualLines[i].FirstDocumentLine.LineNumber % 2 == 0)
                        drawingContext.DrawRectangle(scbEven,
                                                        border,
                                                        new Rect(r.Location, new Size(textView.ActualWidth, r.Height))
                                                        );
                    else
                        
                        drawingContext.DrawRectangle(scbOdd,
                                                            border,
                                                            new Rect(r.Location, new Size(textView.ActualWidth, r.Height))
                                                            );
                    if (odd) odd = false; else odd = true;
                }
            }    //foreach
        }
    }
}
