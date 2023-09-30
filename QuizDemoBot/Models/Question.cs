using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizDemoBot.Models
{
    public class Question
    {
        public string? questionId { get; set; }
        public string MyProperty { get; set; }
        public int? totalPossiblePoint { get; set; }
        public int answerkey;
        //public string AnswerKey { get; set; }

        public string AnswerKey
        {
            get
            {
                return answerkey.ToString();
            }
            set
            {
                int key = 0 + value.ToString().ToCharArray()[0] - 65;
                if (key<4)
                answerkey = key;
            }
        }

        public int isMultipleChoiceQuestion;
        public int includesDiagram { get; set; }
        public string examName { get; set; }
        public int schoolGrade { get; set; }
        public int year { get; set; }
        public string question { get; set; }
        public string subject { get; set; }
        public int? isTest { get; set; }
        public string? isDev { get; set; }

        public IList<string> options { get; set; }

    }
}