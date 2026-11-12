Option Strict Off
Option Explicit On
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Collections.Generic

Public Module DLLConsole
    ' Importowanie funkcji z systemu Windows
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function AllocConsole() As Boolean
    End Function

    Public Sub ShowConsole()
        ' Creates console windows for the process which run emet.dll
        AllocConsole()
    End Sub
End Module

Namespace emetTable
		' solidifying the interface guid
		<Guid("B010E53D-A995-4C01-B802-AABE61A45AAE"), _
				InterfaceType(ComInterfaceType.InterfaceIsIDispatch)> _
			 Public Interface _emetTable
				<DispId(1)> Function PZLFunctionArgs() As Integer
				<DispId(2)> Function PZL(ByRef Feature As Object, ByRef Tm As Double, ByRef Rm As Double, ByRef Angle As Double, ByRef Material As String, ByRef NF As Double) As Double

		End Interface

		' solidifying the clsID
		<Guid("4581135B-2531-4c0d-BA8D-62A60B0712D1"), ClassInterface(ClassInterfaceType.None), _
		ProgId("emet.PZL")> Public Class PZL
				Implements _emetTable

Private Shared ReadOnly LocalizedTexts As New Dictionary(Of String, Dictionary(Of String, String)) From {
    {"en", New Dictionary(Of String, String) From {
				{"script_desc","Program for calculating flat pattern without changing bend radii"},
				{"BendTable","Bend table"},
				{"Thickness","Sheet metal thickness"},
				{"bend_number","Bend number"},
				{"wrongpzl","The difference in radii is too large. I can't calculate it."},
				{"thicknes_not_found","I couldn't find the material thickness in the table."},
				{"big_hem_error","Hem radius is too large. I can't calculate it."},
				{"greater_radius_warning","Warning: it will be bent to a larger radius; the bend reliefs may be too small."},
				{"status_warnings_feature_suffix","Warning, check the console."},
				{"status_error_feature_suffix","Error!"},
				{"r","Bend radius"},
				{"angle","Bend angle"}
    }},
    {"pl", New Dictionary(Of String, String) From {
        {"script_desc", "program do przeliczania rozwinięć bez zmiany promieni gięcia"},
				{"BendTable", "Tabela gięć"},
				{"Thickness", "Grubość blachy"},
				{"bend_number", "Gięcie numer"},
				{"wrongpzl", "Różnica promieni jest zbyt duża. Nie przeliczę tego"},
				{"thicknes_not_found", "Nie znalazłem grubości materiału w tabelce"},
				{"big_hem_error", "Zaklepka ze zbyt dużą szczeliną. Nie przeliczę tego"},
				{"greater_radius_warning", "Uwaga: będzie gięte na większym promieniu, podcięcia mogą okazać się zbyt małe"},
				{"status_warnings_feature_suffix", "uwaga, sprawdź konsolę"},
				{"status_error_feature_suffix", "Błąd!"},
				{"r", "promień gięcia"},
				{"angle", "kąt gięcia"}
    }}
}

Private Shared Function T(ByVal key As String) As String
'AI generated
    Dim lang As String = Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
    If Not LocalizedTexts.ContainsKey(lang) Then lang = "en"
    If Not LocalizedTexts(lang).ContainsKey(key) Then Return key
    Return LocalizedTexts(lang)(key)
End Function

				Private Shared CurrentBendIterator As Integer = 0
				Private Shared Errors As Integer = 0
				Private Shared Warnings As Integer = 0
				Private Shared Config as Dictionary(Of String, Dictionary(Of String, Object))
				Private Shared isIntialized As Boolean = False
				
				Public Sub New()
						'NOTE: In this example there are no default values to set.
						CurrentBendIterator = 0
						Errors = 0
						Warnings = 0
						Config = readExcelToDictionary(My.Application.Info.DirectoryPath & "\emet.xlsx")
						isIntialized = False
				End Sub

				Public Function PZLFunctionArgs() As Integer Implements _emetTable.PZLFunctionArgs
						PZLFunctionArgs = 0
				End Function

Private Function readExcelToDictionary(strFileName) As Dictionary(Of String, Dictionary(Of String, Object))
'AI Generatoed
	Dim result As New Dictionary(Of String, Dictionary(Of String, Object))
	Try
		Dim xlApp As Object = CreateObject("Excel.Application")
		xlApp.Visible = False
		Dim wb As Object = xlApp.Workbooks.Open(strFileName)
		For Each sh As Object In wb.Sheets
			Dim sheetName As String = sh.Name.ToString()
			Dim inner As New Dictionary(Of String, Object)
			Dim used As Object = sh.UsedRange
			Dim rows As Integer = used.Rows.Count
			Dim cols As Integer = used.Columns.Count
			If rows >= 1 AndAlso cols >= 1 Then
				' Read header row (first row) for column names starting from column 2
				Dim headers As New System.Collections.Generic.List(Of String)
				For c As Integer = 2 To cols
					Dim h = used.Cells(1, c).Value
					If h Is Nothing Then
						headers.Add("")
					Else
						headers.Add(h.ToString())
					End If
				Next
				' Data rows start from row 2
				For i As Integer = 2 To rows
					Dim keyObj = used.Cells(i, 1).Value
					If keyObj Is Nothing Then Continue For
					Dim primaryKey As String = keyObj.ToString()
					Dim primaryMap As New Dictionary(Of String, Object)
					For c As Integer = 2 To cols
						Dim headerName As String = headers(c - 2)
						If String.IsNullOrEmpty(headerName) Then Continue For
						Dim cellObj = used.Cells(i, c).Value
						If cellObj Is Nothing Then Continue For
						Dim cellText As String = cellObj.ToString()
						' If cell contains key=value pairs separated by ';' or ',' parse into dictionary
						If cellText.Contains("=") Then
							Dim subMap As New Dictionary(Of String, String)
							Dim parts() As String = cellText.Split({";", ","}, System.StringSplitOptions.RemoveEmptyEntries)
							For Each p As String In parts
								Dim kv() As String = p.Split({"="}, 2, System.StringSplitOptions.None)
								If kv.Length = 2 Then
									Dim kk = kv(0).Trim()
									Dim vv = kv(1).Trim()
									If Not subMap.ContainsKey(kk) Then
										subMap.Add(kk, vv)
									Else
										subMap(kk) = vv
									End If
								End If
							Next
							primaryMap(headerName) = subMap
						Else
							primaryMap(headerName) = cellText
						End If
					Next
					If Not inner.ContainsKey(primaryKey) Then
						inner.Add(primaryKey, primaryMap)
					Else
						inner(primaryKey) = primaryMap
					End If
				Next
			End If
			result(sheetName) = inner
		Next

		wb.Close(False)
		xlApp.Quit()
	Catch ex As Exception
		Console.WriteLine("readExcelToDictionary error: " & ex.Message)
	End Try

	Return result
End Function

Public Function PZL(ByRef Feature As Object, ByRef Tm As Double, ByRef Rm As Double, ByRef Angle As Double, ByRef Material As String, ByRef NF As Double) As Double Implements _emetTable.PZL
	'The function arguments and units are defined as follows:
	'Feature - The SE-Sheet Metal feature that is being created/updated or flattened.
	'Tm- Material thickness in meters. Defined in tools options
	'Rm- Bend radius in meters. Defined by the feature.
	'Angle- Sweep angle of the bend in radians. Defined by the feature
	'NF- Neutral factor value (No units). Define in tools options and on the feature.
	'PZL - Return value of this function. Must be in meters.
	'NOTE: The PZL value that you return to SE-Sheet Metal must be >= 0.000
	ShowConsole()
	If isIntialized = False Or Not Feature.Name.StartsWith("emet.pzl")
		Console.Clear()
		Console.WriteLine("======================================================================================================================")
		Console.WriteLine("emet.pzl, " & T("script_desc") & ", " & BuildInfo.BuildTime & ", Jan Domański, www.emet.pl")
		isIntialized = True
		CurrentBendIterator = 0
	End If
	
	Errors = 0
	Warnings = 0

	if(Feature.Name <> "FORCE") Then
		Feature.Name = "emet.pzl ..."
	End If
		
	Dim thicknessTollerance as Double
	Dim hemGapMax as Double
	Dim hemGapDefault as Double
	Dim radiusTollerance as Double
	
	Dim Tt as Double
	Dim Rt as Double
	Dim Kt as Double
	Dim Kh as Double
	Dim msg as string
	Dim featureStatus as string
	Dim table as string
	Dim isHem as Boolean
		
	isHem = False

	table = Config("config")("default bend table")("value")
	For Each key In Config("materials").Keys
		If (key = Material)
			table = Config("materials")(key)("bend table")
			Exit For
		End If
	Next

	CurrentBendIterator += 1
	Console.WriteLine("")
	Console.WriteLine(T("bend_number") & ": " & CurrentBendIterator)

	thicknessTollerance = Config("config")("thickness tollerance")("value")/1000
	hemGapMax = Config("config")("hem gap max")("value")/1000
	hemGapDefault = Config("config")("hem gap default")("value")/1000
	radiusTollerance = Config("config")("radius tollerance")("value")/1000

	For Each key In Config(table).Keys
		Tt = key/1000
		Rt = Config(table)(key)("R")/1000
		Kt = Config(table)(key)("K")
		Kh = Config(table)(key)("Kh")
		If (Math.Abs(Tt - Tm) < thicknessTollerance) Then Exit For
	Next

	'hem (pl: zaklepka)
	if(Math.Abs(Math.PI - Angle) < 0.001) Then
		isHem = True
	End If

	if (Math.Abs(Tt - Tm) >= thicknessTollerance) Then
		Errors += 1
		msg = T("thicknes_not_found")
		Console.WriteLine(msg)
		MsgBox(msg)
	End If

	if(isHem = True and Rm >= hemGapMax) Then
		Errors += 1
		msg = T("big_hem_error")
		Console.WriteLine(msg)
		MsgBox(msg)
	End If

	if(isHem = False and (Rt-Rm) > radiusTollerance) Then
		Warnings += 1
		Console.WriteLine(T("greater_radius_warning"))
	End If


	If(isHem = True) Then
		Rt = hemGapDefault/2
		Rm = Rt
		Tm = Tt
		Kt = Kh
	End If

	PZL = 2*Math.Tan(Angle/2)*(Rm-Rt+Tm-Tt)+Angle*(Rt+Tt*Kt)
	Console.WriteLine(T("BendTable") & ": " & table & ", " & T("Thickness") & ": " & Math.Round(Tm*1000,3) & " mm" & ", " & T("r") & ": " & Math.Round(Rm*1000,3) & " mm, " & T("angle") & ": " & Math.Round(Angle*180/Math.PI,3) & "°, PZL: " & Math.Round(PZL*1000,3))

	if(errors > 0) Then
		PZL = -1
	End If

	if(PZL < 0) Then
		Console.ForegroundColor = ConsoleColor.Red
		Errors += 1
		msg = T("wrongpzl")
		Console.WriteLine(msg)
		MsgBox(msg)
		Console.ResetColor()
		if(Feature.Name = "FORCE") Then
			PZL = 0
		End If
	End If


	featureStatus = ""
	
	If Errors = 0 Then
		If Warnings = 0 Then
			featureStatus = "OK"
		Else
			featureStatus = T("status_warnings_feature_suffix")
		End If
	Else
		featureStatus = T("status_error_feature_suffix")
	End If

	if(Feature.Name <> "FORCE") Then
		Feature.Name = "emet.pzl " & featureStatus
	End If

	
Exit Function
End Function

#Region "REGISTRATION"

				' Code to register/unregister the additional add-in data was derived from Jason Newell (jasonnewell.net)

				' When Regasm is run on this project, either through IDE or command window, these functions will be called.

				' Setup the required Solid Edge registry values for an addin that were not automatically
				' added due to the guid, progid and com visible attributes.
				<ComRegisterFunctionAttribute()> _
				Public Shared Sub RegisterFunction(ByVal t As Type)

						Dim Key As RegistryKey = Registry.ClassesRoot.CreateSubKey("CLSID\{" + t.GUID.ToString() + "}")
						'   {374704A7-B766-4084-8274-BFAF2BF607BD} = automation object
						If Not (Key Is Nothing) Then
								Key.CreateSubKey("Implemented Categories\" & "{374704A7-B766-4084-8274-BFAF2BF607BD}")
								Key.Close()
						End If
				End Sub

				' Remove any registry values specifically added above.
				<ComUnregisterFunctionAttribute()> _
				Public Shared Sub UnregisterFunction(ByVal t As Type)
						Registry.ClassesRoot.DeleteSubKeyTree("CLSID\{" + t.GUID.ToString() + "}")
				End Sub
#End Region
		End Class

End Namespace
