﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trainer;
using System.IO;
using Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using org.pdfbox.pdmodel;
using org.pdfbox.util;
//using Independentsoft.Odf;

namespace Sentiment_Analyzator.Controller
{
    internal sealed class SentimentAnalyzerController
    {
        public int GetTextForMark(string text)
        {
            Stemmer stemmer = new Stemmer();
            List<string> stemmedWords = new List<string>();
            var words = text.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i< words.Count();i++)
            {
                words[i] = stemmer.Stem(words[i]);
            }
            stemmedWords = words.ToList<string>();
            StreamReader reader = new StreamReader("correlations.txt");
            Dictionary<string,double> correlationTable = new Dictionary<string,double>();
            string line;
            while((line = reader.ReadLine()) !=null)
            {
                double correlation;
                string[] splittedLine = line.Split(' ');
                if (splittedLine.Count() == 2)
                {
                    if (Double.TryParse(splittedLine[1], out correlation) && splittedLine[0] != "")
                    {
                        correlationTable.Add(splittedLine[0], correlation);
                    }
                }
                
            }
            double logOfP = 0;
            foreach (var word in stemmedWords)
            {
                if (correlationTable.ContainsKey(word))
                {
                    logOfP += correlationTable[word];
                }
            }
            double pFraction = Math.Exp(logOfP) ;
            double result = pFraction/(pFraction + 1);
            if (result > 0.5)
            {
                return 1;
            }
            return 0;
        }

        public string GetTextFromFile(string fileName)
        {
            var format = Path.GetExtension(fileName);
            string result = String.Empty;
            switch (format)
            {
                case ".doc":
                    result = GetTextFromDocFile(fileName); break;
                case ".docx": 
                    result = GetTextFromDocFile(fileName); break;
                case ".pdf": 
                    result = GetTextFromPdfFile(fileName);break;
                case ".odt": 
                    result = GetTextFromOdtFile(fileName);break;
                case ".txt": break;
            }
            return result;
        }

        private string GetTextFromDocFile(string fileName)
        {
            string result = String.Empty;
            Microsoft.Office.Interop.Word.Application wordObject;
            wordObject = new Microsoft.Office.Interop.Word.Application();
            object file = fileName; //this is the path
            object nullobject = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Word.Document docs = wordObject.Documents.Open
                (ref file, ref nullobject, ref nullobject, ref nullobject,
                ref nullobject, ref nullobject, ref nullobject, ref nullobject,
                ref nullobject, ref nullobject, ref nullobject, ref nullobject,
                ref nullobject, ref nullobject, ref nullobject, ref nullobject
                                );
            docs.ActiveWindow.Selection.WholeStory();
            docs.ActiveWindow.Selection.Copy();
            IDataObject data = Clipboard.GetDataObject();
            result = data.GetData(DataFormats.Text).ToString();
            docs.Close(ref nullobject, ref nullobject, ref nullobject);
            return result;
        }
        private string GetTextFromPdfFile(string fileName)
        {
            PDDocument doc = PDDocument.load(fileName);
            PDFTextStripper stripper = new PDFTextStripper();
            return stripper.getText(doc);
        }
        private string GetTextFromOdtFile(string fileName)
        {
            //TextDocument odtDoc = new TextDocument(fileName);
            return String.Empty;//odtDoc.ToString();
        }
    }
}
