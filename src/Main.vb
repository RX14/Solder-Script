Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports System
Imports System.IO
Module RunModule

    Sub Main()
        If Command() = "" Then
            GoTo argserror
        End If
        Dim Args As List(Of String) = Command.Split(" ").ToList
        Dim directorylist As Array
        Dim modlist As New List(Of String)

        If Directory.Exists(Args(1)) = True Then
            Try
                Directory.Delete(Args(1))
            Catch
                Console.WriteLine("Please can you empty your tempoary or output directory.")
                'Environment.Exit(0) 'Exit code 0
            End Try
        End If
        If Directory.Exists(Args(2)) = True Then
            Try
                Directory.Delete(Args(2))
            Catch
                Console.WriteLine("Please can you empty your tempoary or output directory.")
                'Environment.Exit(0) 'Exit code 0
            End Try
        End If

        If Args.Count <> 3 Then
argserror:
            Console.WriteLine("Please input correct parameters:" + vbNewLine + vbTab + "1) Mods Directory" + vbNewLine + vbTab + "2) Tempoary Directory (relative)" + vbNewLine + vbTab + "3) Output directory (relative)")
            Environment.Exit(0)
        End If

        directorylist = IO.Directory.GetFiles(Args(0))

        Dim fileparsed As List(Of String)
        For Each i In directorylist
            fileparsed = i.ToString.Split(".").ToList

            If fileparsed(fileparsed.Count - 1) = "zip" Or fileparsed(fileparsed.Count - 1) = "jar" Then
                modlist.Add(i)
            End If
        Next

        Dim filename As String
        Dim modversion As String
        Dim slug As String
        For Each i In modlist
            filename = i.Split("\")(i.Split("\").Length - 1)
            Console.WriteLine("Enter slug for " + filename + " here:")
            slug = Console.ReadLine()
            Console.WriteLine("Enter modversion for " + filename + " here:")
            modversion = Console.ReadLine()
            Directory.CreateDirectory(Args(1))
            Directory.CreateDirectory(Args(2) + "\" + slug + "\")
            FileIO.FileSystem.CopyFile(i, Args(1) + "\mods\" + filename)
            Compress(Args(2) + "\" + slug + "\" + slug + "-" + modversion + ".zip", Args(1) + "\")
            Directory.Delete(Args(1), True)

        Next
    End Sub

End Module
Module Subs
    ' Compresses the files in the nominated folder, and creates a zip file on disk named as outPathname.
    '
    Public Sub Compress(outPathname As String, folderName As String)

        Dim fsOut As FileStream = File.Create(outPathname)
        Dim zipStream As New ZipOutputStream(fsOut)

        zipStream.SetLevel(3)       '0-9, 9 being the highest level of compression
        '        zipStream.Password = password   ' optional. Null is the same as not setting.

        ' This setting will strip the leading part of the folder path in the entries, to
        ' make the entries relative to the starting folder.
        ' To include the full path for each entry up to the drive root, assign folderOffset = 0.
        Dim folderOffset As Integer = folderName.Length + (If(folderName.EndsWith("\"), 0, 1))

        CompressFolder(folderName, zipStream, folderOffset)

        zipStream.IsStreamOwner = True
        ' Makes the Close also Close the underlying stream
        zipStream.Close()
    End Sub

    ' Recurses down the folder structure
    '
    Private Sub CompressFolder(path As String, zipStream As ZipOutputStream, folderOffset As Integer)

        Dim files As String() = Directory.GetFiles(path)

        For Each filename As String In files

            Dim fi As New FileInfo(filename)

            Dim entryName As String = filename.Substring(folderOffset)  ' Makes the name in zip based on the folder
            entryName = ZipEntry.CleanName(entryName)       ' Removes drive from name and fixes slash direction
            Dim newEntry As New ZipEntry(entryName)
            newEntry.DateTime = fi.LastWriteTime            ' Note the zip format stores 2 second granularity

            ' Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
            '   newEntry.AESKeySize = 256;

            ' To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
            ' you need to do one of the following: Specify UseZip64.Off, or set the Size.
            ' If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
            ' but the zip will be in Zip64 format which not all utilities can understand.
            '   zipStream.UseZip64 = UseZip64.Off;
            newEntry.Size = fi.Length

            zipStream.PutNextEntry(newEntry)

            ' Zip the file in buffered chunks
            ' the "using" will close the stream even if an exception occurs
            Dim buffer As Byte() = New Byte(4095) {}
            Using streamReader As FileStream = File.OpenRead(filename)
                StreamUtils.Copy(streamReader, zipStream, buffer)
            End Using
            zipStream.CloseEntry()
        Next
        Dim folders As String() = Directory.GetDirectories(path)
        For Each folder As String In folders
            CompressFolder(folder, zipStream, folderOffset)
        Next
    End Sub
End Module
