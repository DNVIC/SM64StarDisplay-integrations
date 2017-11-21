﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public abstract class Action
    {
        public static string configureName { get { return null; } }

        public abstract int Execute(GraphicsManager gm, int lineOffset); //returns taken size

        public static IEnumerable<Type> GetAllSubclasses()
        {
            var baseType = typeof(Action);
            var assembly = baseType.Assembly;

            return assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
        }
    }

    public class LineDrawAction : Action
    {
        public int Line;
        public byte StarByte;
        public int StarDiff;
        public bool IsSecret;
        public byte StarMask;

        public LineDrawAction(int line, byte starByte, int starDiff, bool isSecret, byte starMask)
        {
            this.Line = line;
            this.StarByte = starByte;
            this.StarDiff = starDiff;
            this.IsSecret = isSecret;
            this.StarMask = starMask;
        }
        
        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            for (int i = 1; i <= 7; i++)
            {
                if ((StarMask & (1 << i)) == 0) continue;
                int x = (IsSecret ? 180 : 0) + i * 20;
                int y = (lineOffset + Line) * 23;
                bool isAcquired = (StarByte & (1 << (i - 1))) != 0;
                Image img = isAcquired ? gm.ld.goldStar : gm.ld.darkStar;
                gm.graphics.DrawImage(img, x, y, 20, 20);
            }
            return 0; //Line Actions should not increase global offset
        }
    }

    public class StarHighlightAction : Action
    {
        public int Line;
        public byte HighlightByte;
        public bool IsSecret;
        public byte StarMask;

        public StarHighlightAction(int line, byte highlightByte, bool isSecret, byte starMask)
        {
            this.Line = line;
            this.HighlightByte = highlightByte;
            this.IsSecret = isSecret;
            this.StarMask = starMask;
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            for (int i = 1; i <= 7; i++)
            {
                if ((StarMask & (1 << i)) == 0) continue;
                int x = (IsSecret ? 180 : 0) + i * 20;
                int y = (lineOffset + Line) * 23;
                bool isAcquired = (HighlightByte & (1 << (i - 1))) != 0;
                //if (isAcquired)
                //    gm.graphics.DrawImage(gm.ld.redOutline, x, y, 20, 20);
            }
            return 0;
        }
    }

    public class RedsSecretsDrawAction : Action
    {
        public int CurrentRedsCount;
        public int TotalRedsCount;
        public int CurrentSecretsCount;
        public int TotalSecretsCount;
        public int ActivePanelsCount;
        public int TotalPanelsCount;
        public new static string configureName { get { return "Show collectables"; } }

        public RedsSecretsDrawAction(int currentRedsCount, int totalRedsCount, int currentSecretsCount, int totalSecretsCount, int activePanelsCount, int totalPanelsCount)
        {
            this.CurrentRedsCount = currentRedsCount < 0 ? 0 : currentRedsCount;
            this.TotalRedsCount = totalRedsCount;
            this.CurrentSecretsCount = currentSecretsCount < 0 ? 0 : currentSecretsCount;
            this.TotalSecretsCount = totalSecretsCount;
            this.ActivePanelsCount = activePanelsCount;
            this.TotalPanelsCount = totalPanelsCount;
        }

        static int totalSize = 30;

        int GetFullSize(int elementsCount)
        {
            return elementsCount * 2;
        }

        int GetTextSize(int elementsCount)
        {
            if (elementsCount <= 2) return 100;
            return elementsCount.ToString().Length * 2 + 3; //3 = space for icon + space for /
        }

        int GetSpaceSize()
        {
            if (TotalPanelsCount == 0 && TotalSecretsCount == 0 && TotalPanelsCount == 0) return 0;
            return (TotalRedsCount != 0 ? 2 : 0) + (TotalSecretsCount != 0 ? 2 : 0) - 2;
        }

        int DrawFullReds(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            for (int i = 0; i < CurrentRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.reds, 20 + i * 20, totalStarLine * 23 + 10, 20, 20);
            }
            for (int i = CurrentRedsCount; i < TotalRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkReds, 20 + i * 20, totalStarLine * 23 + 10, 20, 20);
            }
            return TotalRedsCount * 2 + (TotalRedsCount == 0 ? 0 : 2);
        }

        int DrawFullSecrets(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            for (int i = 0; i < CurrentSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.secrets, 10 * totalSize - i * 20, totalStarLine * 23 + 10, 20, 20);
            }
            for (int i = CurrentSecretsCount; i < TotalSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkSecrets, 10 * totalSize - i * 20, totalStarLine * 23 + 10, 20, 20);
            }
            return TotalSecretsCount * 2 + (TotalSecretsCount == 0 ? 0 : 2);
        }

        int DrawFullFlipswitches(GraphicsManager gm, int offset, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            for (int i = 0; i < ActivePanelsCount; i++)
            {
                gm.graphics.DrawImage(gm.flipswitchOn, i * 20 + 10 * offset + 1, totalStarLine * 23 + 11, 18, 18);
            }
            for (int i = ActivePanelsCount; i < TotalPanelsCount; i++)
            {
                gm.graphics.DrawImage(gm.flipswitchOff, i * 20 + 10 * offset + 1, totalStarLine * 23 + 11, 18, 18);
            }
            return TotalPanelsCount * 2 + (TotalPanelsCount == 0 ? 0 : 2);
        }

        int DrawTextReds(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            string starLine = CurrentRedsCount.ToString() + "/" + TotalRedsCount.ToString();

            SolidBrush redBrush = new SolidBrush(Color.IndianRed);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            Font bigFont = new Font(gm.fontFamily, (gm.drawFontSize + gm.bigFontSize) / 2);

            gm.graphics.DrawImage(gm.reds, 20, totalStarLine * 23 + 10, 20, 20);
            gm.graphics.DrawString(starLine, bigFont, redBrush, 40, totalStarLine * 23 + 10);

            redBrush.Dispose();
            drawBrush.Dispose();

            bigFont.Dispose();
            return starLine.Length + 4;
        }

        int DrawTextSecrets(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            string starLine = CurrentSecretsCount.ToString() + "/" + TotalSecretsCount.ToString();

            SolidBrush blueBrush = new SolidBrush(Color.LightBlue);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            Font bigFont = new Font(gm.fontFamily, (gm.drawFontSize + gm.bigFontSize) / 2);

            gm.graphics.DrawString(starLine, bigFont, blueBrush, 20 + 10 * totalSize - starLine.Length * 10, totalStarLine * 23 + 10);
            gm.graphics.DrawImage(gm.secrets, 10 * totalSize - starLine.Length * 10, totalStarLine * 23 + 10, 20, 20);
            
            blueBrush.Dispose();
            drawBrush.Dispose();

            bigFont.Dispose();
            return starLine.Length + 4;
        }

        int DrawTextFlipswitches(GraphicsManager gm, int offset, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            string starLine = ActivePanelsCount.ToString() + "/" + TotalPanelsCount.ToString();

            SolidBrush redBrush = new SolidBrush(Color.LightGreen);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            Font bigFont = new Font(gm.fontFamily, (gm.drawFontSize + gm.bigFontSize) / 2);

            gm.graphics.DrawImage(gm.flipswitchOff, offset * 10, totalStarLine * 23 + 10, 20, 20);
            gm.graphics.DrawString(starLine, bigFont, redBrush, 20 + offset * 10, totalStarLine * 23 + 10);

            redBrush.Dispose();
            drawBrush.Dispose();

            bigFont.Dispose();
            return starLine.Length + 4;
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            if (!gm.areCollectablesMinimized)
            {
                int[] textSizes = new int[8];
                for (int i = 0; i < 8; i++)
                {
                    int count = 0;
                    count += (i & 1) == 0 ? GetTextSize(TotalSecretsCount) : GetFullSize(TotalSecretsCount);
                    count += (i & 2) == 0 ? GetTextSize(TotalRedsCount) : GetFullSize(TotalRedsCount);
                    count += (i & 4) == 0 ? GetTextSize(TotalPanelsCount) : GetFullSize(TotalPanelsCount);
                    count += GetSpaceSize();
                    if (count > totalSize) count = -1;
                    textSizes[i] = count;
                }
                int maxSize = textSizes.Max();
                int index = Array.FindIndex(textSizes, a => a == maxSize);

                int offset = 2;
                if ((index & 2) == 0)
                    offset += DrawTextReds(gm, lineOffset);
                else
                    offset += DrawFullReds(gm, lineOffset);

                if (maxSize == totalSize) offset -= 1;
                if ((index & 4) == 0)
                    DrawTextFlipswitches(gm, offset, lineOffset);
                else
                    DrawFullFlipswitches(gm, offset, lineOffset);

                if ((index & 1) == 0)
                    DrawTextSecrets(gm, lineOffset);
                else
                    DrawFullSecrets(gm, lineOffset);
            }
            else
            {
                if (TotalRedsCount != 0)
                    DrawTextReds(gm, lineOffset);
                if (TotalSecretsCount != 0)
                    DrawTextSecrets(gm, lineOffset);
            }
            return 2;
        }
    }

    public class LastStarHighlightAction : Action
    {
        public new static string configureName { get { return "Highlight last collected star"; } }
        public LastStarHighlightAction() { }
        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            if (gm.IsFirstCall)
            {
                gm.IsFirstCall = false;
                gm.lastSHA = null;
                return 0;
            }
            if (gm.lastSHA == null) return 0;
            for (int i = 1; i <= 7; i++)
            {
                if ((gm.lastSHA.StarMask & (1 << i)) == 0) continue;
                int x = (gm.lastSHA.IsSecret ? 180 : 0) + i * 20;
                int y = (lineOffset + gm.lastSHA.Line) * 23;
                bool isAcquired = (gm.lastSHA.HighlightByte & (1 << (i - 1))) != 0;
                if (isAcquired)
                {
                    gm.graphics.DrawImage(gm.ld.redOutline, x, y, 20, 20);
                }
            }
            return 0;
        }
    }

    public class TextHighlightAction : Action
    {
        public new static string configureName { get { return "Highlight text for collected items"; } }
        public int Line;
        public bool IsSecret;
        public string Text;
        public TextHighlightAction(int line, bool isSecret, string text)
        {
            Line = line;
            IsSecret = isSecret;
            Text = text;
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            int x = IsSecret ? 180 : 0;
            int y = (lineOffset + Line) * 23;

            SolidBrush drawBrush = new SolidBrush(Color.LightGreen);
            
            Font drawFont = new Font(gm.fontFamily, gm.drawFontSize);

            gm.graphics.DrawString(Text, drawFont, drawBrush, x, y + 3);

            drawBrush.Dispose();
            drawFont.Dispose();

            return 0;
        }
    }

    public class LastHighlight : Action
    {
        public int Line;
        public byte HighlightByte;
        public bool IsSecret;
        public byte StarMask;

        public LastHighlight(int line, byte highlightByte, bool isSecret, byte starMask)
        {
            this.Line = line;
            this.HighlightByte = highlightByte;
            this.IsSecret = isSecret;
            this.StarMask = starMask;
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            gm.lastSHA = this;
            return 0;
        }
    }

    public class DrawCollectablesAction : Action
    {
        public int CurrentRedsCount;
        public int TotalRedsCount;
        public int Line;
        public bool IsText;

        public DrawCollectablesAction(int currentRedsCount, int totalRedsCount, int line, bool isText)
        {
            this.CurrentRedsCount = currentRedsCount < 0 ? 0 : currentRedsCount;
            this.TotalRedsCount = totalRedsCount;
            this.Line = line;
            this.IsText = isText;
        }
        

        void DrawFullReds(GraphicsManager gm, int lineOffset)
        {
            for (int i = 0; i < CurrentRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.reds, 20 + i * 20, (lineOffset + Line) * 23 + 10, 20, 20);
            }
            for (int i = CurrentRedsCount; i < TotalRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkReds, 20 + i * 20, (lineOffset + Line) * 23 + 10, 20, 20);
            }
        }

        void DrawTextReds(GraphicsManager gm, int lineOffset)
        {
            string starLine = CurrentRedsCount.ToString() + "/" + TotalRedsCount.ToString();

            SolidBrush redBrush = new SolidBrush(Color.IndianRed);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            Font bigFont = new Font(gm.fontFamily, 12);

            gm.graphics.DrawImage(gm.reds, 20, (lineOffset + Line) * 23 + 10, 20, 20);
            gm.graphics.DrawString(starLine, bigFont, redBrush, 40, (lineOffset + Line) * 23 + 10);

            redBrush.Dispose();
            drawBrush.Dispose();

            bigFont.Dispose();
        }
        
        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            if (IsText)
                DrawTextReds(gm, lineOffset);
            else
                DrawFullReds(gm, lineOffset);
            return 0;
        }
    }

    public class SecretsDrawAction : Action
    {
        public int CurrentSecretsCount;
        public int TotalSecretsCount;
        public int Line;
        public bool IsText;

        public SecretsDrawAction(int currentRedsCount, int totalRedsCount, int line, bool isText)
        {
            this.CurrentSecretsCount = currentRedsCount < 0 ? 0 : currentRedsCount;
            this.TotalSecretsCount = totalRedsCount;
            this.Line = line;
            this.IsText = isText;
        }


        void DrawFullSecrets(GraphicsManager gm, int lineOffset)
        {
            for (int i = 0; i < CurrentSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.secrets, 200 + i * 20, (lineOffset + Line) * 23 + 10, 20, 20);
            }
            for (int i = CurrentSecretsCount; i < TotalSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkSecrets, 200 + i * 20, (lineOffset + Line) * 23 + 10, 20, 20);
            }
        }

        void DrawTextSecrets(GraphicsManager gm, int lineOffset)
        {
            string starLine = CurrentSecretsCount.ToString() + "/" + TotalSecretsCount.ToString();

            SolidBrush blueBrush = new SolidBrush(Color.LightBlue);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            Font bigFont = new Font(gm.fontFamily, (gm.drawFontSize + gm.bigFontSize) / 2);

            gm.graphics.DrawImage(gm.secrets, 200, (lineOffset + Line) * 23 + 10, 20, 20);
            gm.graphics.DrawString(starLine, bigFont, blueBrush, 220, (lineOffset + Line) * 23 + 11);

            blueBrush.Dispose();
            drawBrush.Dispose();

            bigFont.Dispose();
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            if (IsText)
                DrawTextSecrets(gm, lineOffset);
            else
                DrawFullSecrets(gm, lineOffset);
            return 0;
        }
    }

    public class StarLayoutFiniAction : Action
    {
        public int Length;

        public StarLayoutFiniAction(int length)
        {
            this.Length = length;
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            return Length;
        }
    }

    public class StringLineDrawAction : Action
    {
        public string Line;
        public new static string configureName { get { return "Show collected stars line"; } }

        public StringLineDrawAction(string line)
        {
            this.Line = line;
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            Font bigFont = new Font(gm.fontFamily, gm.bigFontSize);

            RectangleF drawRect = new RectangleF(0, lineOffset * 23, 340, 23);
            StringFormat drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.MeasureTrailingSpaces
            };
            gm.graphics.DrawString(Line, bigFont, drawBrush, drawRect, drawFormat);

            blackBrush.Dispose();
            drawBrush.Dispose();
            return 1;
        }
    }

    public class StarTextLineDrawAction : Action
    {
        public new static string configureName { get { return "Show text line after display"; } }
        public StarTextLineDrawAction() { }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            Font bigFont = new Font(gm.fontFamily, gm.bigFontSize);

            RectangleF drawRect = new RectangleF(0, lineOffset * 23, 340, 23);
            StringFormat drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.MeasureTrailingSpaces
            };
            gm.graphics.DrawString(gm.StarText, bigFont, drawBrush, drawRect, drawFormat);

            blackBrush.Dispose();
            drawBrush.Dispose();
            return 1;
        }
    }

    public class StarLayoutInitAction : Action
    {
        public StarLayoutInitAction()
        {
        }

        public override int Execute(GraphicsManager gm, int lineOffset)
        {
            gm.PaintHUD(lineOffset);
            return 0;
        }
    }

    public class DrawActions : IEnumerable<Action>
    {
        LayoutDescription ld;
        byte[] stars;
        byte[] oldStars;
        byte[] highlightPivot;
        int reds;
        int totalReds;
        int secrets;
        int totalSecrets;
        int activePanels;
        int totalPanels;

        public DrawActions() { }

        public DrawActions(LayoutDescription ld, byte[] stars, byte[] oldStars, byte[] highlightPivot, int reds, int totalReds, int secrets, int totalSecrets, int activePanels, int totalPanels)
        {
            this.ld = ld;
            this.stars = stars;
            this.oldStars = oldStars;
            this.highlightPivot = highlightPivot;
            this.reds = reds;
            this.totalReds = totalReds;
            this.secrets = secrets;
            this.totalSecrets = totalSecrets;
            this.activePanels = activePanels;
            this.totalPanels = totalPanels;
        }

        virtual public IEnumerator<Action> GetEnumerator()
        {
            yield return new StarLayoutInitAction();
            int index; bool isAcquired;
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "B1");
            isAcquired = ((stars[3] & (1 << 4)) != 0) || ((stars[3] & (1 << 6)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "B1");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "B2");
            isAcquired = ((stars[3] & (1 << 5)) != 0) || ((stars[3] & (1 << 7)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "B2");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "WC");
            isAcquired = ((stars[3] & (1 << 1)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "WC");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "MC");
            isAcquired = ((stars[3] & (1 << 2)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "MC");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "VC");
            isAcquired = ((stars[3] & (1 << 3)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "VC");

            for (int line = 0; line < ld.courseDescription.Length; line++)
            {
                var descr = ld.courseDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);
                
                byte diffByteFromPivot = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new StarHighlightAction(line, diffByteFromPivot, false, descr.starMask);
                if (oldStarByte != newStarByte)
                {
                    byte diffbyteFromOld = (byte)(((oldStarByte) ^ (newStarByte)) & newStarByte);
                    yield return new LastHighlight(line, diffbyteFromOld, false, descr.starMask);
                }
            }

            for (int line = 0; line < ld.secretDescription.Length; line++)
            {
                var descr = ld.secretDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);
                
                byte diffByte = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new StarHighlightAction(line, diffByte, true, descr.starMask);
                if (oldStarByte != newStarByte)
                {
                    byte diffbyteFromOld = (byte)(((oldStarByte) ^ (newStarByte)) & newStarByte);
                    yield return new LastHighlight(line, diffbyteFromOld, true, descr.starMask);
                }
            }

            yield return new LastStarHighlightAction();

            int starCount = 0;
            int maxLine = 0;
            for (int line = 0; line < ld.courseDescription.Length; line++)
            {
                var descr = ld.courseDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                if (maxLine < line)
                    maxLine = line;
                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);

                starCount += MemoryManager.countStars((byte)(newStarByte & starMask2));

                byte diffByte = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new LineDrawAction(line, newStarByte, MemoryManager.countStars((byte)(newStarByte & starMask2)) - MemoryManager.countStars((byte)(oldStarByte & starMask2)), false, descr.starMask);
            }

            for (int line = 0; line < ld.secretDescription.Length; line++)
            {
                var descr = ld.secretDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                if (maxLine < line)
                    maxLine = line;
                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);

                starCount += MemoryManager.countStars((byte)(newStarByte & starMask2));

                byte diffByte = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new LineDrawAction(line, newStarByte, MemoryManager.countStars((byte)(newStarByte & starMask2)) - MemoryManager.countStars((byte)(oldStarByte & starMask2)), true, descr.starMask);
            }

            yield return new StarLayoutFiniAction(maxLine + 1);
            yield return new StarTextLineDrawAction();
            yield return new StringLineDrawAction(starCount.ToString().PadLeft(3) + "/" + ld.starAmount.PadRight(3));
            yield return new RedsSecretsDrawAction(reds, totalReds, secrets, totalSecrets, activePanels, totalPanels);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CollectablesOnlyDrawActions : DrawActions
    {
        LayoutDescription ld;
        byte[] stars;
        byte[] oldStars;
        byte[] highlightPivot;
        int reds;
        int totalReds;
        int secrets;
        int totalSecrets;
        int activePanels;
        int totalPanels;

        public CollectablesOnlyDrawActions(LayoutDescription ld, byte[] stars, byte[] oldStars, byte[] highlightPivot, int reds, int totalReds, int secrets, int totalSecrets, int activePanels, int totalPanels)
        {
            this.ld = ld;
            this.stars = stars;
            this.oldStars = oldStars;
            this.highlightPivot = highlightPivot;
            this.reds = reds;
            this.totalReds = totalReds;
            this.secrets = secrets;
            this.totalSecrets = totalSecrets;
            this.activePanels = activePanels;
            this.totalPanels = totalPanels;
        }

        override public IEnumerator<Action> GetEnumerator()
        {
            yield return new DrawCollectablesAction(reds, totalReds, 0, true);
            int line = 1;
            while (totalReds > 0)
            {
                int localTotalReds = totalReds > 8 ? 8 : totalReds;
                int localReds = reds > 8 ? 8 : reds;
                
                yield return new DrawCollectablesAction(localReds, localTotalReds, line, false);

                totalReds -= 8;
                reds -= 8;
                line++;
                if (reds < 0) reds = 0;
            }

            yield return new SecretsDrawAction(secrets, totalSecrets, 0, true);
            line = 1;
            while (totalSecrets > 0)
            {
                int localTotalSecrets = totalSecrets > 8 ? 8 : totalSecrets;
                int localSecrets = secrets > 8 ? 8 : secrets;

                yield return new SecretsDrawAction(localSecrets, localTotalSecrets, line, false);

                totalSecrets -= 8;
                secrets -= 8;
                line++;
                if (secrets < 0) secrets = 0;
            }
        }
    }
}
