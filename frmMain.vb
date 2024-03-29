Imports System.Reflection

Public Class frmMain

    Private m_lookupTable As New Generic.Dictionary(Of String, Double)

    Private Sub cmdCalculate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCalculate.Click
        Try
            Dim strExpr As String
            strExpr = txtInput.Text.Trim

            
            If strExpr = "" Then
                If m_lstPastExpressions.Count > 0 Then
                    strExpr = m_lstPastExpressions.Item(m_lstPastExpressions.Count - 1)
                Else
                    Append("What am I supposed to do?")
                    Exit Sub
                End If
            End If

            m_lstPastExpressions.Add(strExpr)

            
            txtResult.AppendText(strExpr & vbNewLine)

            If strExpr.StartsWith(":") OrElse strExpr.StartsWith(".") Then
                strExpr = strExpr.Substring(1).Trim
                Dim strCmd As String, strArg As String
                If strExpr.Contains(" ") Then
                    strCmd = strExpr.Substring(0, strExpr.IndexOf(" ")).Trim
                    strArg = strExpr.Substring(strExpr.IndexOf(" ")).Trim
                Else
                    strArg = ""
                    strCmd = strExpr
                End If
                Select Case strCmd.ToLower
                    Case "quit", "exit", "q"
                        txtResult.AppendText(vbTab & "Exiting!" & vbNewLine)
                        Close()
                    Case "save", "s"
                        For Each val As Generic.KeyValuePair(Of String, Double) In m_lookupTable
                            SaveSetting(Application.ProductName, "Variables", val.Key, CStr(val.Value))
                            txtResult.AppendText(vbTab & "Saved " & val.Key & " = " & CStr(val.Value) & vbNewLine)
                        Next
                    Case "load", "l"
                        Dim vars(,) As String
                        vars = GetAllSettings(Application.ProductName, "Variables")
                        If vars Is Nothing Then
                            Append("(No saved variables were found)")
                        Else
                            For i As Integer = 0 To vars.GetUpperBound(0)
                                If m_lookupTable.ContainsKey(vars(i, 0)) Then
                                    m_lookupTable(vars(i, 0)) = CDbl(vars(i, 1))
                                Else
                                    m_lookupTable.Add(vars(i, 0), CDbl(vars(i, 1)))
                                End If
                                txtResult.AppendText(vbTab & "Loaded " & vars(i, 0) & " = " & vars(i, 1) & vbNewLine)
                            Next
                        End If
                    Case "deleteall"
                        DeleteSetting(Application.ProductName, "Variables")
                        txtResult.AppendText(vbTab & "All saved variables have been deleted." & vbNewLine)
                    Case "clearall"
                        m_lookupTable.Clear()
                        txtResult.AppendText(vbTab & "All variables have been cleared." & vbNewLine)
                    Case "clear", "c"
                        If strArg = "" Then
                            txtResult.AppendText(vbTab & "Specify a variable to clear!" & vbNewLine)
                        ElseIf m_lookupTable.ContainsKey(strArg) = False Then
                            txtResult.AppendText(vbTab & "Variable '" & strArg & "' wasn't found." & vbNewLine)
                        Else
                            m_lookupTable.Remove(strArg)
                            txtResult.AppendText(vbTab & String.Format("Variable '{0}' cleared.", strArg) & vbNewLine)
                        End If
                    Case "delete", "d"
                        If strArg = "" Then
                            txtResult.AppendText(vbTab & "Specify a variable to delete!" & vbNewLine)
                        Else
                            Try
                                DeleteSetting(Application.ProductName, "Variables", strArg)
                                txtResult.AppendText(vbTab & String.Format("Variable '{0}' deleted.", strArg) & vbNewLine)
                            Catch
                                txtResult.AppendText(vbTab & "Variable '" & strArg & "' wasn't found." & vbNewLine)
                            End Try
                        End If
                    Case "list"
                        If strArg = "saved" Then
                            Dim vars(,) As String
                            vars = GetAllSettings(Application.ProductName, "Variables")
                            If vars Is Nothing Then
                                txtResult.AppendText(vbTab & "(no saved variables)" & vbNewLine)
                            Else
                                For i As Integer = 0 To vars.GetUpperBound(0)
                                    txtResult.AppendText(vbTab & vars(i, 0) & " = " & vars(i, 1) & vbNewLine)
                                Next
                            End If
                        ElseIf strArg = "" OrElse strArg = "loaded" Then
                            If m_lookupTable.Count = 0 Then
                                txtResult.AppendText(vbTab & "(no loaded variables)" & vbNewLine)
                            Else
                                For Each val As Generic.KeyValuePair(Of String, Double) In m_lookupTable
                                    txtResult.AppendText(vbTab & val.Key & " = " & CStr(val.Value) & vbNewLine)
                                Next
                            End If
                        Else
                            txtResult.AppendText(vbTab & "Arguments can be: 'Saved' or 'Loaded'. If no argument is specified, 'Loaded' is assumed." & vbNewLine)
                        End If
                    Case Else
                        If Not (strCmd = "?" OrElse strCmd = "help" OrElse strCmd = "h" OrElse strCmd = "") Then
                            txtResult.AppendText(vbTab & "Unknown command: " & strCmd & vbNewLine)
                        End If
                        Append("Commands available:")
                        Append(vbTab & "quit (short form: q).                   Exit.")
                        Append(vbTab & "exit                                    Same as quit.")
                        Append(vbTab & "save (short form: s).                   Save all the variables to the registry.")
                        Append(vbTab & "load (short form: l).                   Load all the variables from the registry.")
                        Append(vbTab & "clear <variablename> (short form: c).   Clear the specified loaded variable.")
                        Append(vbTab & "clearall                                Clear all the loaded variables.")
                        Append(vbTab & "delete <variablename> (short form: d).  Delete the specified saved variable in the registry.")
                        Append(vbTab & "deleteall                               Delete all the variables saved in the registry.")
                        Append(vbTab & "list <saved><loaded>                    Lists all the saved / loaded variables.")
                End Select
            Else
                Dim result As String
                result = Calculate(strExpr)
                If result = "" Then
                    txtResult.AppendText(vbTab & "<no result>" & vbNewLine)
                Else
                    txtResult.AppendText(vbTab & result & vbNewLine)
                End If
            End If
            m_lstCurrentExpr = -1
            txtInput.Text = ""
        Catch ex As Exception
            txtResult.AppendText(vbTab & ex.Message & vbNewLine)
        End Try
    End Sub
    Private Sub Append(ByVal text As String)
        txtResult.AppendText(vbTab & text & vbNewLine)
    End Sub
    Private Sub TextBox2_DownClick() Handles txtInput.DownClick
        Try
            If m_lstCurrentExpr = -1 Then
                m_lstCurrentExpr = 0
            ElseIf m_lstCurrentExpr < m_lstPastExpressions.Count - 1 Then
                m_lstCurrentExpr += 1
            Else
                m_lstCurrentExpr = 0
            End If
            If m_lstPastExpressions.Count > 0 Then
                txtInput.Text = m_lstPastExpressions.Item(m_lstCurrentExpr)
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub TextBox2_UpClick() Handles txtInput.UpClick
        Try
            If m_lstCurrentExpr = -1 Then
                m_lstCurrentExpr = m_lstPastExpressions.Count - 1
            ElseIf m_lstCurrentExpr > 0 Then
                m_lstCurrentExpr -= 1
            Else
                m_lstCurrentExpr = m_lstPastExpressions.Count - 1
            End If
            If m_lstPastExpressions.Count > 0 Then
                txtInput.Text = m_lstPastExpressions.Item(m_lstCurrentExpr)
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Private m_lstPastExpressions As New Generic.List(Of String)
    Private m_lstCurrentExpr As Integer
    Private Function Calculate(ByVal str As String) As String
        Dim result As String = ""
        Dim tokens As Generic.List(Of Token)

        Try
            str = str.Trim
            If str = "" Then
                Throw New ArgumentException("Expression to calculate is empty!")
            End If

            tokens = Token.Tokenize(str)

            Dim p As New ParseTree
            p.Parse(tokens)
            Dim calcResult As Double
            calcResult = p.Calculate(m_lookupTable)
            If p.isAssignment Then
                Return "Variable " & p.strAssingVariable & " has been set to " & calcResult.ToString
            Else
                If m_lookupTable.ContainsKey("answer") Then
                    m_lookupTable("answer") = calcResult
                Else
                    m_lookupTable.Add("answer", calcResult)
                End If
                Return calcResult.ToString
            End If
        Catch ex As ApplicationException
            result = ex.Message
            Throw
            If ex.Message = "Exiting!" Then
                Close()
            Else
                result = ex.Message
            End If
        Catch ex As Exception
            result = ex.Message
        End Try

        Return result
    End Function

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            txtInput.Text = ".s"
            cmdCalculate_Click(Nothing, Nothing)
        Catch ex As Exception
            MsgBox("Error: " & ex.Message)
        End Try
    End Sub

    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            txtInput.Text = ".l"
            cmdCalculate_Click(Nothing, Nothing)
        Catch ex As Exception
            MsgBox("Error: " & ex.Message)
        End Try
    End Sub
End Class

Class TextBox2
    Inherits TextBox
    Public Event UpClick()
    Public Event DownClick()
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Const WM_KEYDOWN As Integer = &H100
        If m.Msg = WM_KEYDOWN Then
            Select Case CInt(m.WParam)
                Case Keys.Up
                    RaiseEvent UpClick()
                    m.Result = IntPtr.Zero
                Case Keys.Down
                    RaiseEvent DownClick()
                    m.Result = IntPtr.Zero
                Case Else
                    MyBase.WndProc(m)
            End Select
        Else
            MyBase.WndProc(m)
        End If
    End Sub

End Class

Class Token
    Private Shared operators As Char() = [Operator].ops ' New Char() {"+"c, "-"c, "*"c, "/"c, "^"c, "!"c}
    Private Shared symbols As Char() = Symbol.syms
    Private Shared numbers As Char() = New Char() {"0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "."c}
    Protected value As Object

    Shared Function isLetter(ByVal c As Char) As Boolean
        Return (Asc(c) >= Asc("a"c) AndAlso Asc(c) <= Asc("z"c)) OrElse (Asc(c) >= Asc("A"c) AndAlso Asc(c) <= Asc("Z"c))
    End Function
    Shared Function isLetterOrNumber(ByVal c As Char) As Boolean
        Return isLetter(c) OrElse isNumber(c)
    End Function
    Shared Function isSymbol(ByVal c As Char) As Boolean
        Return Array.IndexOf(symbols, c) >= 0
    End Function
    Shared Function isNumber(ByVal c As Char) As Boolean
        Return Array.IndexOf(numbers, c) >= 0
    End Function
    Shared Function isOperator(ByVal c As Char) As Boolean
        Return Array.IndexOf(operators, c) >= 0
    End Function
    Shared Function Tokenize(ByVal str As String) As Generic.List(Of Token)
        Dim result As New Generic.List(Of Token)

        str = str & ";"

        Dim curChar, nextChar As Char
        For i As Integer = 0 To str.Length - 2
            curChar = str.Chars(i)
            nextChar = str.Chars(i + 1)
            If isOperator(curChar) Then
                result.Add(New [Operator](curChar))
            ElseIf isNumber(curChar) Then
                Dim iFirst, iLast As Integer
                iFirst = i
                Do Until isNumber(nextChar) = False
                    i += 1
                    nextChar = str.Chars(i + 1)
                Loop
                iLast = i
                Dim strNumber As String = Mid(str, iFirst + 1, iLast - iFirst + 1)
                If strNumber.Contains(".") Then
                    result.Add(New RealNumber(strNumber))
                Else
                    result.Add(New IntNumber(strNumber))
                End If
            ElseIf isSymbol(curChar) Then
                result.Add(New Symbol(curChar))
            ElseIf curChar = ";"c Then
                GoTo endParsing
            ElseIf curChar = " "c OrElse curChar = CChar(vbTab) Then
                'skip 
            ElseIf isLetter(curChar) Then
                Dim iFirst, iLast As Integer
                iFirst = i
                Do Until isLetterOrNumber(nextChar) = False AndAlso nextChar <> "_"c
                    i += 1
                    nextChar = str.Chars(i + 1)
                Loop
                iLast = i
                Dim strId As String = Mid(str, iFirst + 1, iLast - iFirst + 1)
                result.Add(New Identifier(strId))
            Else
                Throw New ArgumentException("Character '" & curChar & "' is not valid.")
            End If
        Next
endParsing:

        Return result
    End Function
    Shared Operator =(ByVal tok1 As Identifier, ByVal tok2 As Token) As Boolean
        If tok2 Is Nothing Then
            If tok1 Is Nothing Then
                Return True
            Else
                Return False
            End If
        ElseIf tok1 Is Nothing Then
            Return False 'Tok2 isnot nothing
        Else
            If TypeOf tok2 Is Identifier Then
                Return DirectCast(tok2, Identifier).getValue = tok1.getValue
            Else
                Return False
            End If
        End If
    End Operator
    Shared Operator <>(ByVal tok1 As Identifier, ByVal tok2 As Token) As Boolean
        Return Not tok1 = tok2
    End Operator
    Shared Operator =(ByVal tok1 As Symbol, ByVal tok2 As Token) As Boolean
        If tok2 Is Nothing Then
            If tok1 Is Nothing Then
                Return True
            Else
                Return False
            End If
        ElseIf tok1 Is Nothing Then
            Return False 'Tok2 isnot nothing
        Else
            If TypeOf tok2 Is Symbol Then
                Return DirectCast(tok2, Symbol).getValue = tok1.getValue
            Else
                Return False
            End If
        End If
    End Operator
    Shared Operator <>(ByVal tok1 As Symbol, ByVal tok2 As Token) As Boolean
        Return Not tok1 = tok2
    End Operator
    Shared Operator =(ByVal tok1 As Symbol.Symbols, ByVal tok2 As Token) As Boolean
        If tok2 Is Nothing Then
            Return False
        Else
            If TypeOf tok2 Is Symbol Then
                Return DirectCast(tok2, Symbol).getValue = tok1
            Else
                Return False
            End If
        End If
    End Operator
    Shared Operator <>(ByVal tok1 As Symbol.Symbols, ByVal tok2 As Token) As Boolean
        Return Not tok1 = tok2
    End Operator
End Class
Class Symbol
    Inherits Token
    Public Shared ReadOnly syms As Char() = New Char() {"("c, ")"c, "="c, ","c}
    Enum Symbols
        LParenthesis
        RParenthesis
        Equals
        Comma
    End Enum
    Sub New(ByVal symbol As Char)
        value = DirectCast(Array.IndexOf(syms, symbol), Symbols)
    End Sub
    Shared Function symtostr(ByVal sym As Symbols) As String
        Return syms(CInt(sym))
    End Function
    Function symtostr() As String
        Return symtostr(getValue)
    End Function
    Public Function getValue() As Symbols
        Return DirectCast(value, Symbols)
    End Function
End Class
MustInherit Class Number
    Inherits Token
    MustOverride Sub SetNegative()
End Class
Class IntNumber
    Inherits Number

    Sub New(ByVal number As String)
        Dim int As Integer
        If Integer.TryParse(number, int) Then
            value = int
        Else
            Throw New ArgumentException("'" & number & "' is not a valid integer number.")
        End If
    End Sub
    Function getValue() As Integer
        Return CInt(value)
    End Function

    Public Overrides Sub SetNegative()
        value = -getValue()
    End Sub
End Class
Class RealNumber
    Inherits Number
    Sub New(ByVal number As String)
        Dim dbl As Double
        If Double.TryParse(number, Globalization.NumberStyles.AllowDecimalPoint, Globalization.CultureInfo.GetCultureInfo("en-us").NumberFormat, dbl) Then
            value = dbl
        Else
            Throw New ArgumentException("'" & number & "' is not a valid real number.")
        End If
    End Sub
    Function getValue() As Double
        Return CDbl(value)
    End Function

    Public Overrides Sub SetNegative()
        value = -getValue()
    End Sub
End Class
Class [Operator]
    Inherits Token
    Enum Operators
        Addition = 0
        Subtraction
        Multiplication
        Division
        Power
        Factorial
    End Enum
    Public Shared ReadOnly ops As Char() = New Char() {"+"c, "-"c, "*"c, "/"c, "^"c, "!"c}
    Public Shared Function optostr(ByVal op As Operators) As String
        Return ops(CInt(op))
    End Function
    Public Function optostr() As String
        Return optostr(Me.getValue)
    End Function
    Sub New(ByVal op As Char)
        Select Case op
            Case "+"c
                value = Operators.Addition
            Case "-"c
                value = Operators.Subtraction
            Case "*"c
                value = Operators.Multiplication
            Case "/"c
                value = Operators.Division
            Case "^"c
                value = Operators.Power
            Case "!"c
                value = Operators.Factorial
            Case Else
                Throw New ArgumentException(String.Format("'{0}' is not a valid operator.", op))
        End Select
    End Sub
    Function getValue() As [Operator].Operators
        Return DirectCast(value, [Operator].Operators)
    End Function
End Class
Class Identifier
    Inherits Token
    Sub New(ByVal id As String)
        value = id
    End Sub
    Function getValue() As String
        Return DirectCast(value, String)
    End Function
End Class
Class FunctionCall
    Inherits Token
    Public Args As New Generic.List(Of ParseLeaf)
    Public Func As Identifier
End Class
Class ParseTree
    Private FirstLeaf As ParseLeaf

    Private tokens As Generic.Queue(Of Token)
    Private isAssign As Boolean
    Private assignTo As String
    Private currentToken As Token
    Private Function nextToken() As Token
        If tokens.Count > 0 Then
            currentToken = tokens.Dequeue
        Else
            currentToken = Nothing
        End If
        Return currentToken
    End Function
    Public Function Parse(ByVal tokens As Generic.List(Of Token)) As ParseTree
        Dim result As New ParseTree

        Me.tokens = New Generic.Queue(Of Token)(tokens)
        nextToken()

        If TypeOf currentToken Is Identifier AndAlso Me.tokens.Count >= 1 AndAlso TypeOf Me.tokens.Peek Is Symbol AndAlso DirectCast(Me.tokens.Peek, Symbol).getValue = Symbol.Symbols.Equals Then
            'Assignment
            assignTo = DirectCast(currentToken, Identifier).getValue
            isAssign = True
            nextToken()
            nextToken() 'skip the = as well
        End If

        If currentToken Is Nothing Then
            Throw New ArgumentException("And the expression, where is it?")
        Else
            FirstLeaf = DoFirst()
        End If

        Return result
    End Function
    Public ReadOnly Property isAssignment() As Boolean
        Get
            Return isAssign
        End Get
    End Property
    Public ReadOnly Property strAssingVariable() As String
        Get
            Return assignTo
        End Get
    End Property
    Public Function Calculate(ByVal LookupTable As Generic.Dictionary(Of String, Double)) As Double

        Calculate = FirstLeaf.Calculate(LookupTable)
        If isAssign Then
            If LookupTable.ContainsKey(assignTo.ToLower) Then
                LookupTable.Item(assignTo.ToLower) = Calculate
            Else
                LookupTable.Add(assignTo.ToLower, Calculate)
            End If
        End If
    End Function
    Private Function DoValue() As ParseLeaf
        Dim rValue As ParseLeaf

        If TypeOf currentToken Is Number Then
            rValue = New ParseLeaf
            rValue.Value = currentToken
            nextToken()
        ElseIf TypeOf currentToken Is Symbol Then
            Dim symb As Symbol
            symb = DirectCast(currentToken, Symbol)
            If symb.getValue = Symbol.Symbols.LParenthesis Then
                nextToken()
                rValue = DoFirst()
                symb = TryCast(currentToken, Symbol)
                If symb IsNot Nothing AndAlso symb.getValue = Symbol.Symbols.RParenthesis Then
                    nextToken()
                Else
                    Throw New ArgumentException("Expected ')'.")
                    Stop ' "(" but no ")"
                End If
            Else
                Throw New ArgumentException("Expected number or '(', not ')'.")
                Stop ' ")" ??
            End If
        ElseIf TypeOf currentToken Is Identifier Then
            Dim id As Identifier = DirectCast(currentToken, Identifier)
            rValue = New ParseLeaf
            nextToken()
            If currentToken IsNot Nothing AndAlso TypeOf currentToken Is Symbol AndAlso DirectCast(currentToken, Symbol).getValue = Symbol.Symbols.LParenthesis Then
                'A function
                Dim newFunction As New FunctionCall
                Dim symb As Symbol

                rValue.Value = newFunction
                newFunction.Func = id

                nextToken()
                symb = TryCast(currentToken, Symbol)
                If Symbol.Symbols.RParenthesis = symb Then
                    nextToken()
                Else
                    Dim arg As ParseLeaf
                    Do
                        arg = DoFirst()
                        newFunction.Args.Add(arg)
                        symb = TryCast(currentToken, Symbol)
                        nextToken()
                    Loop While Symbol.Symbols.Comma = symb
                    If Symbol.Symbols.RParenthesis <> symb Then
                        Throw New ArgumentException("Expected ')'.")
                    Else
                        nextToken()
                    End If
                End If
            Else
                'Just a variable
                rValue.Value = id
            End If
        ElseIf currentToken Is Nothing Then
            Throw New ArgumentException("Unexpected end of the expression.")
        ElseIf TypeOf currentToken Is [Operator] Then
            Dim op As [Operator] = DirectCast(currentToken, [Operator])

            Throw New ArgumentException(String.Format("{0} operator ({1}) is not valid here.", op.getValue.ToString, op.optostr))
        Else
            Stop
            Throw New ArgumentException("Unexpected token: " & currentToken.GetType.ToString)
            rValue = New ParseLeaf
        End If

        Return rValue
    End Function
    Private Function DoNegative() As ParseLeaf
        Dim op As [Operator] = TryCast(currentToken, [Operator])
        If op IsNot Nothing AndAlso op.getValue = [Operator].Operators.Subtraction Then
            Dim tmp As ParseLeaf
            nextToken()
            tmp = DoValue()
            If TypeOf tmp.Value Is Number Then
                DirectCast(tmp.Value, Number).SetNegative()
                Return tmp
            Else
                Dim tmp2 As New ParseLeaf
                tmp2.RightLeaf = tmp
                tmp2.LeftLeaf = New ParseLeaf
                tmp2.LeftLeaf.Value = New IntNumber("-1")
                tmp2.Value = New [Operator]("*"c)
                Return tmp2
            End If
        Else
            Return DoValue()
        End If
    End Function
    Private Function DoFactorial() As ParseLeaf
        Dim rValue As ParseLeaf

        rValue = DoNegative()

        Dim op As [Operator] = TryCast(currentToken, [Operator])
        If op IsNot Nothing Then
            If op.getValue = [Operator].Operators.Factorial Then
                Dim leaf As New ParseLeaf
                leaf.LeftLeaf = rValue
                leaf.Value = op
                nextToken()
                rValue = leaf
            Else
                Return rValue
            End If
        End If
        Return rValue
    End Function
    Private Function DoPower() As ParseLeaf
        Dim rValue As ParseLeaf

        rValue = DoFactorial()

        Dim op As [Operator] = TryCast(currentToken, [Operator])
        Do While op IsNot Nothing
            If op.getValue = [Operator].Operators.Power Then
                Dim leaf As New ParseLeaf
                leaf.LeftLeaf = rValue
                leaf.Value = op
                nextToken()
                leaf.RightLeaf = DoFactorial()
                rValue = leaf
            Else
                Return rValue
            End If
            op = TryCast(currentToken, [Operator])
        Loop
        Return rValue
    End Function
    Private Function DoMultDiv() As ParseLeaf
        Dim rValue As ParseLeaf

        rValue = DoPower()

        Dim op As [Operator] = TryCast(currentToken, [Operator])
        Do While op IsNot Nothing
            If op.getValue = [Operator].Operators.Division OrElse op.getValue = [Operator].Operators.Multiplication Then
                Dim leaf As New ParseLeaf
                leaf.LeftLeaf = rValue
                leaf.Value = op
                nextToken()
                leaf.RightLeaf = DoPower()
                rValue = leaf
            Else
                Return rValue
            End If
            op = TryCast(currentToken, [Operator])
        Loop
        Return rValue
    End Function
    Private Function DoAddSub() As ParseLeaf
        Dim rValue As ParseLeaf

        rValue = DoMultDiv()

        Dim op As [Operator] = TryCast(currentToken, [Operator])
        Do While op IsNot Nothing
            If op.getValue = [Operator].Operators.Addition OrElse op.getValue = [Operator].Operators.Subtraction Then
                Dim leaf As New ParseLeaf
                leaf.LeftLeaf = rValue
                leaf.Value = op
                nextToken()
                leaf.RightLeaf = DoMultDiv()
                rValue = leaf
            Else
                Return rValue
            End If
            op = TryCast(currentToken, [Operator])
        Loop
        Return rValue
    End Function
    Private Function DoFirst() As ParseLeaf
        Return DoAddSub()
    End Function
End Class
Class ParseLeaf
    Public LeftLeaf As ParseLeaf
    Public RightLeaf As ParseLeaf
    Public Value As Token
    Public Function Calculate(ByVal LookupTable As Generic.Dictionary(Of String, Double)) As Double
        If TypeOf Value Is RealNumber Then
            Debug.Assert(LeftLeaf Is Nothing AndAlso RightLeaf Is Nothing)
            Return DirectCast(Value, RealNumber).getValue
        ElseIf TypeOf Value Is IntNumber Then
            Debug.Assert(LeftLeaf Is Nothing AndAlso RightLeaf Is Nothing)
            Return DirectCast(Value, IntNumber).getValue
        ElseIf TypeOf Value Is Symbol Then
            Stop '?? Shouldn't happen
        ElseIf TypeOf Value Is [Operator] Then
            Dim op As [Operator] = DirectCast(Value, [Operator])
            Select Case op.getValue
                Case [Operator].Operators.Addition
                    Debug.Assert(LeftLeaf IsNot Nothing AndAlso RightLeaf IsNot Nothing)
                    Return LeftLeaf.Calculate(LookupTable) + RightLeaf.Calculate(LookupTable)
                Case [Operator].Operators.Division
                    Debug.Assert(LeftLeaf IsNot Nothing AndAlso RightLeaf IsNot Nothing)
                    Return LeftLeaf.Calculate(LookupTable) / RightLeaf.Calculate(LookupTable)
                Case [Operator].Operators.Multiplication
                    Debug.Assert(LeftLeaf IsNot Nothing AndAlso RightLeaf IsNot Nothing)
                    Return LeftLeaf.Calculate(LookupTable) * RightLeaf.Calculate(LookupTable)
                Case [Operator].Operators.Power
                    Debug.Assert(LeftLeaf IsNot Nothing AndAlso RightLeaf IsNot Nothing)
                    Return LeftLeaf.Calculate(LookupTable) ^ RightLeaf.Calculate(LookupTable)
                Case [Operator].Operators.Subtraction
                    Debug.Assert(LeftLeaf IsNot Nothing AndAlso RightLeaf IsNot Nothing)
                    Return LeftLeaf.Calculate(LookupTable) - RightLeaf.Calculate(LookupTable)
                Case [Operator].Operators.Factorial
                    Debug.Assert(LeftLeaf IsNot Nothing AndAlso RightLeaf Is Nothing)
                    Dim res As Double
                    Dim last As Integer
                    Dim tmp As Double = LeftLeaf.Calculate(LookupTable)
                    If Math.Truncate(tmp) = tmp Then
                        last = CInt(tmp)
                    Else
                        Throw New ArgumentException("Cannot do factorial on a number with decimals.")
                    End If
                    If last < 0 Then
                        Throw New ArgumentException("Cannot do factorial on a negative number.")
                    End If
                    If last = 0 Then
                        Return 0
                    End If
                    res = 1
                    For i As Integer = 2 To last
                        res *= i
                    Next
                    Return res
                Case Else
                    Throw New ArgumentException("Invalid operator: " & op.getValue.ToString)
            End Select
        ElseIf TypeOf Value Is Identifier Then
            Dim id As Identifier = DirectCast(Value, Identifier)
            If LookupTable.ContainsKey(id.getValue.ToLower) Then
                Return LookupTable.Item(id.getValue.ToLower)
            Else
                Dim math As Type = GetType(System.Math)
                Dim fld As FieldInfo
                fld = math.GetField(id.getValue, BindingFlags.IgnoreCase Or BindingFlags.Public Or BindingFlags.Static)
                If fld Is Nothing Then
                    Throw New ArgumentException(String.Format("Identifier {0} not found.", id.getValue))
                Else
                    If fld.FieldType.Equals(GetType(Double)) = False Then
                        Throw New ArgumentException(String.Format("Identifier {0} not found.", id.getValue))
                    Else
                        Return CDbl(fld.GetValue(Nothing))
                    End If
                End If
            End If
        ElseIf TypeOf Value Is FunctionCall Then
            Dim func As FunctionCall = DirectCast(Value, FunctionCall)
            Dim math As Type = GetType(System.Math)
            Dim allmethods As MethodInfo() = math.GetMethods()
            Dim methods As New Generic.List(Of MethodInfo)

            'Select the method(s)
            For Each meth As MethodInfo In allmethods
                If StrComp(meth.Name, func.Func.getValue, CompareMethod.Text) = 0 Then
                    Dim params As ParameterInfo()
                    params = meth.GetParameters()
                    If params.Length = func.Args.Count Then
                        'Is all parameters double? - exact match!
                        Dim bAllDouble As Boolean = True
                        For Each par As ParameterInfo In params
                            If par.ParameterType IsNot GetType(Double) Then
                                bAllDouble = False
                                Exit For
                            End If
                        Next
                        If bAllDouble = True Then
                            methods.Clear()
                            methods.Add(meth)
                            Exit For
                        Else
                            'not all doubles.
                            Dim bValidParam As Boolean = True
                            For Each par As ParameterInfo In params
                                Select Case Type.GetTypeCode(par.ParameterType)
                                    Case TypeCode.Decimal, TypeCode.Double, _
                                         TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, _
                                         TypeCode.Single
                                    Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Char, TypeCode.DateTime, TypeCode.DBNull, _
                                         TypeCode.Object, TypeCode.SByte, _
                                         TypeCode.String, _
                                         TypeCode.Empty, _
                                         TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64
                                        'not a valid type
                                        bValidParam = False
                                        Exit For
                                End Select
                            Next
                            If bValidParam = True Then methods.Add(meth)
                        End If
                    End If
                End If
            Next
            If methods.Count = 0 Then
                Throw New ArgumentException("Function '" & func.Func.getValue & "' was not found with " & func.Args.Count & " parameters.")
            ElseIf methods.Count = 1 Then
                Dim method As MethodInfo = methods(0)
                Dim params As Object()
                ReDim params(func.Args.Count - 1)
                For i As Integer = 0 To func.Args.Count - 1
                    params(i) = func.Args(i).Calculate(LookupTable)
                Next
                Dim result As Object
                result = method.Invoke(Nothing, params)
                If result Is Nothing Then
                    Throw New ArgumentException("Function '" & func.Func.getValue & "' didn't return a value.")
                ElseIf IsNumeric(result) = False Then
                    Throw New ArgumentException("Function '" & func.Func.getValue & "' didn't return a numeric value! Returned a value of type = " & result.GetType.ToString)
                Else
                    Return CDbl(result)
                End If
            Else
                Throw New ArgumentException("Several possible functions '" & func.Func.getValue & "' was found with " & func.Args.Count & " parameters.")
            End If
        Else
            Throw New ArgumentException("Leaf with type = " & Value.GetType.ToString & " is unknown?")
        End If
    End Function
End Class