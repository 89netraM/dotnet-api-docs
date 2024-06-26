﻿'<snippet1>
Imports System.Text

Class EncodingExample
  Public Shared Sub Main()
      ' Create and ASCII encoding.
      Dim ascii As Encoding = Encoding.ASCII

      ' A Unicode string with two characters outside the ASCII code range.
      Dim unicodeString As String = "This unicode string contains two characters " + "with codes outside the ASCII code range, " + "Pi (" & ChrW(&H03A0) & ") and Sigma (" & ChrW(&H03A3) & ")."
      Console.WriteLine("Original string:")
      Console.WriteLine(unicodeString)

      ' Save the positions of the special characters for later reference.
      Dim indexOfPi As Integer = unicodeString.IndexOf(ChrW(&H03A0))
      Dim indexOfSigma As Integer = unicodeString.IndexOf(ChrW(&H03A3))

      ' Encode the string.
      Dim encodedBytes As Byte() = ascii.GetBytes(unicodeString)
      Console.WriteLine()
      Console.WriteLine("Encoded bytes:")
      For Each b In encodedBytes
          Console.Write("[{0}]", b)
      Next b
      Console.WriteLine()

      ' Notice that the special characters have been replaced with
      ' the value 63, which is the ASCII character code for '?'.
      Console.WriteLine()
      Console.WriteLine("Value at position of Pi character: {0}", encodedBytes(indexOfPi))
      Console.WriteLine("Value at position of Sigma character: {0}", encodedBytes(indexOfSigma))

      ' Decode bytes back to a string.
      ' Notice missing Pi and Sigma characters.
      Dim decodedString As String = ascii.GetString(encodedBytes)
      Console.WriteLine()
      Console.WriteLine("Decoded bytes:")
      Console.WriteLine(decodedString)
  End Sub
End Class
'This code produces the following output.
'Original string:
'This unicode string contains two characters with codes outside the ASCII code range, Pi (Π) and Sigma (Σ).
'
'Encoded bytes:
'[84][104][105][115][32][117][110][105][99][111][100][101][32]'[115][116][114][105][110][103][32][99][111][110][116][97]'[105][110][115][32][116][119][111][32][99][104][97][114][97]'[99][116][101][114][115][32][119][105][116][104][32][99][111]'[100][101][115][32][111][117][116][115][105][100][101][32]'[116][104][101][32][65][83][67][73][73][32][99][111][100]'[101][32][114][97][110][103][101][44][32][80][105][32][40]'[63][41][32][97][110][100][32][83][105][103][109][97][32][40]'[63][41][46]
'
'Value at position of Pi character: 63
'Value at position of Sigma character: 63
'
'Decoded bytes:
'This unicode string contains two characters with codes outside 'the ASCII code range, Pi (?) and Sigma (?).
'
'</snippet1>