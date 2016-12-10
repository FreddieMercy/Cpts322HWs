using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Collections;
//using System.Drawing;
using System.Xml;

namespace SpreadSheetEngine
{
    //Cpts 322 HW 7 by Junhao Zhang"Freddie" ID#: 11356533

    public abstract class UndoRedoUnit
    {
        protected Spreadsheet addr;
        protected int row;
        protected int col;
        protected string text = "";
        public UndoRedoUnit(ref Spreadsheet tmp, int _row, int _col, string t)
        {
            addr = tmp;
            row = _row;
            col = _col;
            text = t;
        }

        public virtual void perform()
        {

        }

        public string Text
        {
            get
            {
                return text;
            }
        }
    }

    public class URColor : UndoRedoUnit
    {
        private int bf;
        private int after;
        public URColor(ref Spreadsheet tmp, int _row, int _col, int sth, int sb)
            : base(ref tmp, _row, _col, " the cell color")
        {
            bf = sth;
            after = sb;
        }

        public override void perform()
        {
            int color = bf;
            addr.GetCell(row, col).Color = bf;
            bf = after;
            after = color;
        }
    }

    public class URValue : UndoRedoUnit
    {
        private string bf;
        private string af;

        public URValue(ref Spreadsheet tmp, int _row, int _col, string sth, string sb)
            : base(ref tmp, _row, _col, " the cell text")
        {
            bf = sth;
            af = sb;
        }

        public override void perform()
        {
            string s = bf;
            Cell tmp = (Cell)addr.GetCell(row, col);
            tmp.noKnow();
            addr.GetCell(row, col).Textss = "";
            addr.GetCell(row, col).Text = bf;
            bf = af;
            af = s;
        }
    }


    public class UndoRedo
    {
        private Stack<UndoRedoUnit> undo = new Stack<UndoRedoUnit>();
        private Stack<UndoRedoUnit> redo = new Stack<UndoRedoUnit>();
        private UndoRedoUnit cur = null;
        private Spreadsheet view;

        private Hashtable once = new Hashtable();
        public UndoRedo(ref Spreadsheet addr)
        {
            view = addr;

        }

        public Hashtable getOnce
        {
            get
            {
                return once;
            }
        }

        private void AddOrSet(string key, string c)
        {
            if(once.Contains(key))
            {
                once[key] = c;
            }
            else
            {
                once.Add(key, c);
            }
        }

        //Track the changes
        public void Push(int row, int col, string e, string sh, string sb)
        {

            //For some weird reasons both " XmlSerializer" and "IXmlSerializable" don't work, hence I have to do it "manually"
            string key = null;
            string c = "<Cell row =\"" + row.ToString() + "\" col=\"" + col.ToString() + "\">";
            if (e == "Color")
            {
                key = "Color";
                cur = new URColor(ref view, row, col, Int32.Parse(sh), Int32.Parse(sb));
                c += "<BGColor>"+sb+"</BGColor>";
                
            }
            else if (e == "Value")
            {
                key = "Text";
                cur = new URValue(ref view, row, col, sh, sb);
                c += "<Text>" + sb + "</Text>";
            }

            c += "</Cell>";
            if(key!=null)
            {
                AddOrSet(row.ToString()+col.ToString()+key, c);
            }
            undo.Push(cur);
            redo.Clear();
        }

        public void Undo()
        {
            if (!UndoIsEmpty())
            {
                UndoRedoUnit tmp = undo.Pop();
                tmp.perform();
                redo.Push(tmp);
            }
        }

        public void Redo()
        {
            if (!RedoIsEmpty())
            {
                UndoRedoUnit tmp = redo.Pop();
                tmp.perform();
                undo.Push(tmp);
            }
        }

        public bool UndoIsEmpty()
        {
            if (undo.Count >= 1)
            {
                return false;
            }

            return true;
        }

        public bool RedoIsEmpty()
        {
            if (redo.Count >= 1)
            {
                return false;
            }

            return true;
        }

        public UndoRedoUnit getUndoTop()
        {
            return undo.Peek();
        }
        public UndoRedoUnit getRedoTop()
        {
            return redo.Peek();
        }
    }
    public abstract class Node
    {
        protected int _height;
        private string _name;
        private Node _leftChild;
        private Node _rightChild;

        public Node(string name = " ", int height = 0, Node left = null, Node right = null)
        {
            _height = height;
            _name = name;
            _leftChild = left;
            _rightChild = right;
        }

        public virtual float evaluate()
        {
            return 0;
        }
        public string getName
        {
            get
            {
                return _name;
            }
        }

        public Node LeftChild
        {
            set
            {
                _leftChild = value;
            }

            get
            {
                return _leftChild;
            }
        }

        public Node RightChild
        {
            set
            {
                _rightChild = value;
            }

            get
            {
                return _rightChild;
            }
        }


        public int Height
        {
            set
            {
                _height = value;
            }

            get
            {
                return _height;
            }
        }

    }

    class BaseNode : Node
    {
        protected string _value;

        public BaseNode(string value = " ", string name = " ", int height = -1, Node left = null, Node right = null)
            : base(name, height, left, right)
        {
            _value = value;
        }

        public virtual float getNumbricValue
        {
            get
            {
                return float.Parse(_value);
            }
        }

        public override float evaluate()
        {
            return this.getNumbricValue;
        }

    }

    class VariableNode : BaseNode
    {

        public VariableNode(string value = " ", string name = " ", int height = -1, Node left = null, Node right = null)
            : base(value, name, height, left, right)
        {

        }

        public VariableNode(Cell sth, string name = " ", int height = -1, Node left = null, Node right = null)
            : base(sth.Text, name, height, left, right)
        {
            sth.PropertyChanged += onTextChange;
        }

        private void onTextChange(object sender, PropertyChangedEventArgs e)
        {
            Cell s = sender as Cell;
            this._value = s.Text;

        }

        public override float getNumbricValue
        {
            get
            {
                if (Regex.IsMatch(_value.ToString(), @"^[0-9]") == true)
                {
                    return float.Parse(this._value);
                }
                else
                {
                    return float.Parse("1/0");
                }
            }
        }
    }

    class ConstantNode : BaseNode
    {

        public ConstantNode(string value = " ", int height = -1, Node left = null, Node right = null)
            : base(value, value, height, left, right)
        {

        }
    }

    class OpNode : Node
    {
        private string _op;
        public OpNode(string value = " ", int height = 0, Node left = null, Node right = null)
            : base(value, height, left, right)
        {
            _op = value;
            if (_op == "+" || _op == "-")
            {
                _height = 1;
            }

            left = null;
            right = null;
        }
        // it is a construct function to evaluate all the nodes under root, handy. When it has parameters, I will write another construct loop under the Node class to evaluate them.
        public ConstantNode calculate(OpNode sth)
        {
            ConstantNode baseLeft;
            ConstantNode baseRight;
            if ((sth.LeftChild.LeftChild != null) && (sth.LeftChild.RightChild != null))
            {
                OpNode l = (OpNode)sth.LeftChild;
                baseLeft = sth.calculate(l);
            }
            else
            {
                if ((Regex.IsMatch(sth.LeftChild.getName[0].ToString(), @"^[a-zA-Z]") == true) || (sth.LeftChild.getName == " "))
                {
                    VariableNode l = (VariableNode)sth.LeftChild;
                    baseLeft = new ConstantNode(l.getNumbricValue.ToString());
                }
                else
                {
                    ConstantNode l = (ConstantNode)sth.LeftChild;
                    baseLeft = new ConstantNode(l.getNumbricValue.ToString());
                }

            }

            if ((sth.RightChild.LeftChild != null) && (sth.RightChild.RightChild != null))
            {
                OpNode r = (OpNode)sth.RightChild;
                baseRight = sth.calculate(r);
            }
            else
            {
                if ((Regex.IsMatch(sth.RightChild.getName[0].ToString(), @"^[a-zA-Z]") == true) || (sth.RightChild.getName == " "))
                {
                    VariableNode r = (VariableNode)sth.RightChild;
                    baseRight = new ConstantNode(r.getNumbricValue.ToString());
                }
                else
                {
                    ConstantNode r = (ConstantNode)sth.RightChild;
                    baseRight = new ConstantNode(r.getNumbricValue.ToString());
                }
            }

            float result;

            switch (sth._op)
            {
                case "+":
                    result = baseLeft.getNumbricValue + baseRight.getNumbricValue;
                    break;

                case "-":
                    result = baseLeft.getNumbricValue - baseRight.getNumbricValue;
                    break;

                case "*":
                    result = baseLeft.getNumbricValue * baseRight.getNumbricValue;
                    break;

                case "/":
                    result = baseLeft.getNumbricValue / baseRight.getNumbricValue;
                    break;

                default:
                    result = 0;
                    break;

            }
            ConstantNode tmp = new ConstantNode(result.ToString());
            return tmp;
        }

        public override float evaluate()
        {
            ConstantNode tmp = calculate(this);
            return tmp.getNumbricValue;
        }
    }

    // default "internal"
    class ExpressionTree
    {
        private string _exp;
        private Node _root;
        private Cell[,] _variables;

        public ExpressionTree(ref Cell[,] sth, string exp = "")
        {
            _variables = sth;
            _exp = exp;
            _root = null;
        }

        public string Expression
        {
            set
            {
                _exp = value;
            }
        }

        private void sort(ref string s, ref Cell sb, ref List<Node> nodes, string t = "")
        {
            Node var = null;
            OpNode op;
            if (s != "")
            {
                //Better error handling.
                if ((s.Length <= 3) && (Regex.IsMatch(s[0].ToString(), @"^[a-zA-Z]") == true) && (Regex.IsMatch(s.Substring(1, s.Length - 1), @"^[0-9]") == true))
                {
                    int col = Convert.ToInt32(s[0]) - 65;
                    int row = Convert.ToInt32(s.Substring(1, s.Length - 1)) - 1;
                    if (_variables[row, col].Text != null)//&&(Regex.IsMatch(_variables[row, col].Text, @"^[0-9]") == true))
                    {
                        var = new VariableNode(_variables[row, col], s);
                        sb.getToKnow(ref _variables[row, col]);
                    }
                }
                else if (Regex.IsMatch(s, @"^[0-9]") == true)
                {
                    var = new ConstantNode(s);
                }

                if ((nodes.Count != 0) && (nodes[nodes.Count - 1].Height == 0))
                {
                    nodes[nodes.Count - 1].RightChild = var;
                }
                else
                {
                    nodes.Add(var);
                }

                s = "";
            }

            if (t != "")
            {
                op = new OpNode(t);
                nodes.Add(op);
            }

        }

        private Node sort_again(ref List<Node> nodes)
        {

            for (int i = 1; i < nodes.Count; i++)
            {
                if ((nodes[i - 1].Height <= nodes[i].Height) && (nodes[i].Height != 1))
                {
                    nodes[i].LeftChild = nodes[i - 1];
                    nodes.RemoveAt(i - 1);
                    i--;
                }

            }

            for (int i = nodes.Count - 2; i >= 0; i--)
            {
                if (nodes[i].Height > nodes[i + 1].Height)
                {
                    nodes[i].RightChild = nodes[i + 1];
                    nodes.RemoveAt(i + 1);
                    i--;
                }
            }

            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i - 1].Height <= nodes[i].Height)
                {
                    nodes[i].LeftChild = nodes[i - 1];
                    nodes.RemoveAt(i - 1);
                    i--;
                }
            }

            return nodes[0];

        }

        //For my algrithm of evaluating the expression with parentheses, I kept the content inside the parentheses and evaluate them all together at the end, 
        //because I saved the ref of the cell which is the value of the variable so if the value changed the variable node can easily know
        //

        private Node test(ref string t, ref Cell sb, int sth = 0)
        {
            List<Node> nodes = new List<Node>();
            Node _base;
            string s = "";
            for (int i = sth; i < t.Length; i++)
            {
                //I said I will build the "function inside itself" to handle the parentheses (sorry I forgot the terminology) 
                if (t[i] == '(')
                {
                    nodes.Add(test(ref t, ref sb, i + 1));
                    //sort(ref s, ref nodes);
                }
                else if (t[i] == ')')
                {
                    sort(ref s, ref sb, ref nodes);
                    _base = sort_again(ref nodes);
                    try
                    {
                        _root = (OpNode)_base;
                    }
                    catch (InvalidCastException)
                    {
                        _root = (BaseNode)_base;
                    }
                    t = t.Remove(sth, i - sth + 1);
                    _root.Height = -1;
                    return _root;
                }
                else
                {
                    if ((t[i] != '+') && (t[i] != '*') && (t[i] != '-') && (t[i] != '/'))
                    {
                        s += t[i];
                    }
                    else
                    {
                        sort(ref s, ref sb, ref nodes, t[i].ToString());
                    }
                }

            }

            sort(ref s, ref sb, ref nodes);

            _base = sort_again(ref nodes);
            try
            {
                _root = (OpNode)_base;
            }
            catch (InvalidCastException)
            {
                _root = (BaseNode)_base;
            }

            return _root;

        }

        public Node getRoot(ref Cell sth)
        {
            string s = _exp;
            return test(ref s, ref sth);
        }
    }


    // It is the "Cell" class 
    abstract public class SpreadCell : INotifyPropertyChanged
    {
        protected string text;
        protected string _value;
        private int RowIndex;
        private int ColumnIndex;
        private int BGColor;

        public event PropertyChangedEventHandler PropertyChanged;
        public SpreadCell(int color, int _row = 0, int _col = 0)
        {
            RowIndex = _row;
            ColumnIndex = _col;
            text = "";
            _value = "";
            BGColor = color;
        }

        //Color getter/setter
        public int Color
        {
            set
            {
                if (BGColor != value)
                {
                    BGColor = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
                }
            }
            get
            {
                return BGColor;
            }
        }

        //Text property, hope it looks like "SpreadCell.Text = ??"
        public virtual string Text
        {
            get
            {
                return text;
            }

            set
            {
                // I was inspired by the "Hint": hence the Text property does the check to assign the right value to the right individual :P
                // Improvement to make the cell doesn't change anything if the value is the same
                if ((value != null) && (value != "") && (value[0].ToString() == "="))
                {
                    if (_value != value)
                    {

                        _value = value;
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                    else
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                    }
                }
                else if (value != text)
                {

                    this.text = value;
                    // consist the value with the text, if no value function had been set.
                    this._value = this.text;
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }

            }
        }

        public virtual string Texts
        {
            set
            {
                this.text = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public virtual string Textss
        {
            set
            {
                this.text = value;
                this._value = this.text;
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public string Value
        {
            get
            {
                if (String.IsNullOrEmpty(_value))
                {
                    return " ";
                }
                return this._value;
            }
        }

        //Row index # getter but not setter
        public int RowIndexs
        {
            get
            {
                return RowIndex;
            }

        }

        //Column # getter but not setter

        public int ColuIndexs
        {
            get
            {
                return ColumnIndex;
            }

        }

    }

    //A derived class so I can use the "SpreadCell" class
    public class Cell : SpreadCell
    {
        private Node _root;
        private List<Cell> cells = new List<Cell>();

        public void getToKnow(ref Cell cell)
        {
            cell.PropertyChanged += eventHappened;
            cells.Add(cell);
        }

        public void noKnow()
        {
            foreach (Cell cell in cells)
            {
                cell.PropertyChanged -= eventHappened;
            }

            cells.Clear();
        }

        public Cell(int color, int _row = 0, int _col = 0)
            : base(color, _row, _col)
        {

        }



        public Node root
        {
            set
            {
                _root = value;
            }

            get
            {
                return _root;
            }
        }

        private void eventHappened(object sender, PropertyChangedEventArgs e)
        {

            if (_root != null)
            {
                try
                {
                    this.Texts = this.root.evaluate().ToString();
                }
                catch (Exception)
                {
                    this.Texts = "Error";
                }
            }

        }

    }

    // This is the "Spreadsheet" class
    public class Spreadsheet
    {
        private Cell[,] cellCollection;
        private ExpressionTree tree;
        public event PropertyChangedEventHandler CellPropertyChanged;

        protected void OnPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler cc = CellPropertyChanged;
            if (cc != null)
            {
                handler(sender, e);
            }
        }

        public Spreadsheet(int color, int row = 50, int col = 26)
        {
            cellCollection = new Cell[row, col];

            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < col; c++)
                {
                    cellCollection[r, c] = new Cell(color, r, c);
                    cellCollection[r, c].PropertyChanged += OnPropertyChangedEventHandler;
                }
            }

            tree = new ExpressionTree(ref cellCollection);
        }

        public SpreadCell GetCell(int row, int col)
        {
            try
            {
                return cellCollection[row, col];
            }
            catch (IndexOutOfRangeException)
            {
                return cellCollection[49, 25];
            }
        }

        public int ColumnCount
        {
            get
            {
                return cellCollection.GetLength(1);
            }
        }


        public int RowCount
        {
            get
            {
                return cellCollection.GetLength(0);
            }
        }

        private void handler(object sender, PropertyChangedEventArgs e)
        {
            Cell s = sender as Cell;

            if (e.PropertyName == "Value")
            {
                s.noKnow();
                try
                {
                    //Deleted some useless code... that was stupid
                    tree.Expression = s.Value.Substring(1, s.Value.Length - 1);
                    s.root = tree.getRoot(ref s);
                    try
                    {
                        s.Texts = s.root.evaluate().ToString();
                    }
                    catch (Exception)
                    {
                        s.Texts = "Error";
                    }
                }
                catch (Exception)
                {
                    s.Texts = "Error";
                }
            }

            CellPropertyChanged(sender, e);
        }

    }
}