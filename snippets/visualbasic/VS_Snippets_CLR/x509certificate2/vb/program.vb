'<Snippet1>
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.IO
Imports System.Text


' To run this sample use the Certificate Creation Tool (Makecert.exe) to generate a test X.509 certificate and
' place it in the local user store.
' To generate an exchange key and make the key exportable run the following command from a Visual Studio command prompt:
'makecert -r -pe -n "CN=CERT_SIGN_TEST_CERT" -b 01/01/2010 -e 01/01/2012 -sky exchange -ss my

Class Program

    ' Path variables for source, encryption, and
    ' decryption folders. Must end with a backslash.
    Private Shared encrFolder As String = "C:\Encrypt\"
    Private Shared decrFolder As String = "C:\Decrypt\"
    Private Shared originalFile As String = "TestData.txt"
    Private Shared encryptedFile As String = "TestData.enc"


    Shared Sub Main(ByVal args() As String)

        ' Create an input file with test data.
        Dim sw As StreamWriter = File.CreateText(originalFile)
        sw.WriteLine("Test data to be encrypted")
        sw.Close()

        ' Get the certificate to use to encrypt the key.
        Dim cert As X509Certificate2 = GetCertificateFromStore("CN=CERT_SIGN_TEST_CERT")
        If cert Is Nothing Then
            Console.WriteLine("Certificate 'CN=CERT_SIGN_TEST_CERT' not found.")
            Console.ReadLine()
        End If


        ' Encrypt the file using the public key from the certificate.
        EncryptFile(originalFile, CType(cert.PublicKey.Key, RSA))

        ' Decrypt the file using the private key from the certificate.
        DecryptFile(encryptedFile, cert.GetRSAPrivateKey())

        'Display the original data and the decrypted data.
        Console.WriteLine("Original:   {0}", File.ReadAllText(originalFile))
        Console.WriteLine("Round Trip: {0}", File.ReadAllText(decrFolder + originalFile))
        Console.WriteLine("Press the Enter key to exit.")
        Console.ReadLine()

    End Sub

    '<Snippet2>
    Private Shared Function GetCertificateFromStore(ByVal certName As String) As X509Certificate2
        ' Get the certificate store for the current user.
        Dim store As New X509Store(StoreLocation.CurrentUser)
        Try
            store.Open(OpenFlags.ReadOnly)

            ' Place all certificates in an X509Certificate2Collection object.
            Dim certCollection As X509Certificate2Collection = store.Certificates
            ' If using a certificate with a trusted root you do not need to FindByTimeValid, instead use:
            ' currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, true);
            Dim currentCerts As X509Certificate2Collection = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, False)
            Dim signingCert As X509Certificate2Collection = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, False)
            If signingCert.Count = 0 Then
                Return Nothing
            End If ' Return the first certificate in the collection, has the right name and is current.
            Return signingCert(0)
        Finally
            store.Close()
        End Try


    End Function 'GetCertificateFromStore

    '</Snippet2>
    ' <Snippet3>
    ' Encrypt a file using a public key.
    Private Shared Sub EncryptFile(ByVal inFile As String, ByVal rsaPublicKey As RSA)
        Dim aes As Aes = Aes.Create()
        Try
            ' Create instance of Aes for
            ' symmetric encryption of the data.
            aes.KeySize = 256
            aes.Mode = CipherMode.CBC
            Dim transform As ICryptoTransform = aes.CreateEncryptor()
            Try
                Dim keyFormatter As New RSAPKCS1KeyExchangeFormatter(rsaPublicKey)
                Dim keyEncrypted As Byte() = keyFormatter.CreateKeyExchange(aes.Key, aes.GetType())

                ' Create byte arrays to contain
                ' the length values of the key and IV.
                Dim LenK(3) As Byte
                Dim LenIV(3) As Byte

                Dim lKey As Integer = keyEncrypted.Length
                LenK = BitConverter.GetBytes(lKey)
                Dim lIV As Integer = aes.IV.Length
                LenIV = BitConverter.GetBytes(lIV)

                ' Write the following to the FileStream
                ' for the encrypted file (outFs):
                ' - length of the key
                ' - length of the IV
                ' - encrypted key
                ' - the IV
                ' - the encrypted cipher content
                Dim startFileName As Integer = inFile.LastIndexOf("\") + 1
                ' Change the file's extension to ".enc"
                Dim outFile As String = encrFolder + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc"
                Directory.CreateDirectory(encrFolder)

                Dim outFs As New FileStream(outFile, FileMode.Create)
                Try

                    outFs.Write(LenK, 0, 4)
                    outFs.Write(LenIV, 0, 4)
                    outFs.Write(keyEncrypted, 0, lKey)
                    outFs.Write(aes.IV, 0, lIV)

                    ' Now write the cipher text using
                    ' a CryptoStream for encrypting.
                    Dim outStreamEncrypted As New CryptoStream(outFs, transform, CryptoStreamMode.Write)
                    Try

                        ' By encrypting a chunk at
                        ' a time, you can save memory
                        ' and accommodate large files.
                        Dim count As Integer = 0

                        ' blockSizeBytes can be any arbitrary size.
                        Dim blockSizeBytes As Integer = aes.BlockSize / 8
                        Dim data(blockSizeBytes) As Byte
                        Dim bytesRead As Integer = 0

                        Dim inFs As New FileStream(inFile, FileMode.Open)
                        Try
                            Do
                                count = inFs.Read(data, 0, blockSizeBytes)
                                outStreamEncrypted.Write(data, 0, count)
                                bytesRead += count
                            Loop While count > 0
                            inFs.Close()
                        Finally
                            inFs.Dispose()
                        End Try
                        outStreamEncrypted.FlushFinalBlock()
                        outStreamEncrypted.Close()
                    Finally
                        outStreamEncrypted.Dispose()
                    End Try
                    outFs.Close()
                Finally
                    outFs.Dispose()
                End Try
            Finally
                transform.Dispose()
            End Try
        Finally
            aes.Dispose()
        End Try

    End Sub


    ' </Snippet3>
    ' <Snippet4>
    ' Decrypt a file using a private key.
    Private Shared Sub DecryptFile(ByVal inFile As String, ByVal rsaPrivateKey As RSA)

        ' Create instance of Aes for
        ' symmetric decryption of the data.
        Dim aes As Aes = Aes.Create()
        Try
            aes.KeySize = 256
            aes.Mode = CipherMode.CBC

            ' Create byte arrays to get the length of
            ' the encrypted key and IV.
            ' These values were stored as 4 bytes each
            ' at the beginning of the encrypted package.
            Dim LenK() As Byte = New Byte(4 - 1) {}
            Dim LenIV() As Byte = New Byte(4 - 1) {}

            ' Consruct the file name for the decrypted file.
            Dim outFile As String = decrFolder + inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt"

            ' Use FileStream objects to read the encrypted
            ' file (inFs) and save the decrypted file (outFs).
            Dim inFs As New FileStream(encrFolder + inFile, FileMode.Open)
            Try

                inFs.Seek(0, SeekOrigin.Begin)
                inFs.Seek(0, SeekOrigin.Begin)
                inFs.Read(LenK, 0, 3)
                inFs.Seek(4, SeekOrigin.Begin)
                inFs.Read(LenIV, 0, 3)

                ' Convert the lengths to integer values.
                Dim lengthK As Integer = BitConverter.ToInt32(LenK, 0)
                Dim lengthIV As Integer = BitConverter.ToInt32(LenIV, 0)

                ' Determine the start postition of
                ' the cipher text (startC)
                ' and its length(lenC).
                Dim startC As Integer = lengthK + lengthIV + 8
                Dim lenC As Integer = (CType(inFs.Length, Integer) - startC)

                ' Create the byte arrays for
                ' the encrypted AES key,
                ' the IV, and the cipher text.
                Dim KeyEncrypted() As Byte = New Byte(lengthK - 1) {}
                Dim IV() As Byte = New Byte(lengthIV - 1) {}

                ' Extract the key and IV
                ' starting from index 8
                ' after the length values.
                inFs.Seek(8, SeekOrigin.Begin)
                inFs.Read(KeyEncrypted, 0, lengthK)
                inFs.Seek(8 + lengthK, SeekOrigin.Begin)
                inFs.Read(IV, 0, lengthIV)
                Directory.CreateDirectory(decrFolder)
                '<Snippet10>
                ' Use RSA
                ' to decrypt the AES key.
                Dim KeyDecrypted As Byte() = rsaPrivateKey.Decrypt(KeyEncrypted, RSAEncryptionPadding.Pkcs1)

                ' Decrypt the key.
                Dim transform As ICryptoTransform = aes.CreateDecryptor(KeyDecrypted, IV)
                '</Snippet10>
                ' Decrypt the cipher text from
                ' from the FileSteam of the encrypted
                ' file (inFs) into the FileStream
                ' for the decrypted file (outFs).
                Dim outFs As New FileStream(outFile, FileMode.Create)
                Try
                    ' Decrypt the cipher text from
                    ' from the FileSteam of the encrypted
                    ' file (inFs) into the FileStream
                    ' for the decrypted file (outFs).

                    Dim count As Integer = 0

                    Dim blockSizeBytes As Integer = aes.BlockSize / 8
                    Dim data(blockSizeBytes) As Byte

                    ' By decrypting a chunk a time,
                    ' you can save memory and
                    ' accommodate large files.
                    ' Start at the beginning
                    ' of the cipher text.
                    inFs.Seek(startC, SeekOrigin.Begin)
                    Dim outStreamDecrypted As New CryptoStream(outFs, transform, CryptoStreamMode.Write)
                    Try
                        Do
                            count = inFs.Read(data, 0, blockSizeBytes)
                            outStreamDecrypted.Write(data, 0, count)
                        Loop While count > 0

                        outStreamDecrypted.FlushFinalBlock()
                        outStreamDecrypted.Close()
                    Finally
                        outStreamDecrypted.Dispose()
                    End Try
                    outFs.Close()
                Finally
                    outFs.Dispose()
                End Try
                inFs.Close()

            Finally
                inFs.Dispose()

            End Try

        Finally
            aes.Dispose()
        End Try


    End Sub
End Class
' </Snippet4>

'</Snippet1>
