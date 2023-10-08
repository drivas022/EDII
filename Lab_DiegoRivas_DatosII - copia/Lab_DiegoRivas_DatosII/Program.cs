﻿using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Text;

//C:\Users\driva\OneDrive - Universidad Rafael Landivar\Escritorio\Lab_DiegoRivas_DatosII\Lab_DiegoRivas_DatosII\inputl2.csv
//7686671562353
//4981559841093
class Program
{
    private static BTree bTree = new BTree(9);  // Asumimos un grado t=3 para el árbol B

    static void Main()
    {
        LoadCommandsFromFile(@"C:\Users\driva\OneDrive - Universidad Rafael Landivar\Escritorio\Lab_DiegoRivas_DatosII\Lab_DiegoRivas_DatosII\inputl2.csv");  // Reemplazar con la ruta correcta al archivo de comandos

        // Permitir que el usuario busque registros
        UserSearch();

        // ... (resto del código, como la lectura inicial de datos.txt, etc.)
    }

    private static void LoadCommandsFromFile(string path)
    {
        LZ77 lz77 = new LZ77();  // Instancia de la clase LZ77 para codificación y decodificación

        using (StreamReader sr = new StreamReader(path))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                // Decodificar la línea usando LZ77
                string decodedLine = lz77.Decode(lz77.Encode(line));

                JObject jsonObject = JObject.Parse(decodedLine.Substring(decodedLine.IndexOf("{")));  // Ignorar el prefijo (INSERT, PATCH, DELETE) al parsear

                if (line.StartsWith("INSERT;"))
                {
                    string json = line.Substring(7);
                    Record user = JsonConvert.DeserializeObject<Record>(json);

                    // Codificar DPI y Companies antes de almacenar
                    user.DPI = JsonConvert.SerializeObject(lz77.Encode(user.DPI));
                    for (int i = 0; i < user.Companies.Count; i++)
                    {
                        user.Companies[i] = JsonConvert.SerializeObject(lz77.Encode(user.Companies[i]));
                    }

                    bTree.Insert(user);
                }
                else if (line.StartsWith("PATCH;"))
                {
                    string json = line.Substring(6);
                    Record newUser = JsonConvert.DeserializeObject<Record>(json);

                    // Codificar el DPI ingresado
                    string encodedDpi = JsonConvert.SerializeObject(lz77.Encode(newUser.DPI));

                    // Buscar por DPI codificado
                    Record existingUser = bTree.SearchByDpi(encodedDpi);

                    if (existingUser != null)
                    {
                        // Actualizar los campos del registro existente con valores codificados

                        // Codificar el DPI de newUser antes de actualizar
                        existingUser.DPI = encodedDpi;

                        // Actualizar los demás campos (que no requieren codificación)
                        existingUser.DateBirth = newUser.DateBirth;
                        existingUser.Address = newUser.Address;

                        // Codificar cada compañía de newUser antes de actualizar
                        List<string> encodedCompanies = newUser.Companies
                            .Select(company => JsonConvert.SerializeObject(lz77.Encode(company)))
                            .ToList();
                        existingUser.Companies = encodedCompanies;
                    }
                    else if (line.StartsWith("DELETE;"))
                    {
                        string j = line.Substring(7);
                        Record user = JsonConvert.DeserializeObject<Record>(j);

                        // Codificar el DPI ingresado
                        LZ77 encoder = new LZ77();
                        string encoDPI = JsonConvert.SerializeObject(encoder.Encode(user.DPI));

                        // Eliminar por DPI codificado
                        bTree.Delete(encoDPI);
                    }
                }
            }
        }
    }
    public static string ConvertTokensToString(List<LZ77Token> tokens)
    {
        return string.Join("", tokens.Select(t => $"<{t.Distance}, {t.Length}, {t.NextChar}>"));
    }
    private static void UserSearch()
    {
        LZ77 encoder = new LZ77();  // Instancia del codificador

        Console.WriteLine("Ingresa un DPI para buscar o escribe 'exit' para salir: ");
        string dpi;
        while ((dpi = Console.ReadLine()) != "exit")
        {
            // Codificar el DPI ingresado
            List<LZ77Token> encodedDpiTokens = encoder.Encode(dpi);

            // Convertir tokens a la cadena en formato deseado
            string customString = ConvertTokensToString(encodedDpiTokens);
            Console.WriteLine($"DPI Codificado: {customString}");

            // Convertir la lista de tokens a un formato que el árbol pueda usar para buscar
            string encodedDpiForSearch = JsonConvert.SerializeObject(encodedDpiTokens);

            Record user = bTree.SearchByDpi(encodedDpiForSearch);
            if (user != null)
            {
                // Decodificar el DPI antes de mostrarlo
                user.DPI = encoder.Decode(JsonConvert.DeserializeObject<List<LZ77Token>>(user.DPI));

                // Decodificar las compañías y convertirlas al formato deseado
                for (int i = 0; i < user.Companies.Count; i++)
                {
                    List<LZ77Token> companyTokens = JsonConvert.DeserializeObject<List<LZ77Token>>(user.Companies[i]);
                    user.Companies[i] = ConvertTokensToString(companyTokens);
                }

                // Mostrar el usuario en formato JSON con indentación
                string userJson = JsonConvert.SerializeObject(user, Formatting.Indented);
                Console.WriteLine($"Registro encontrado:\n{userJson}");
            }
            else
            {
                Console.WriteLine($"Ningun registro encontrado con el DPI {dpi}");
            }
            Console.WriteLine("Ingresa otro DPI para buscar o escribe 'exit' para salir: ");
        }
    }
    /*private static void UserSearch()
    {
        LZ77 encoder = new LZ77();  // Instancia del codificador

        Console.WriteLine("Ingresa un DPI para buscar o escribe 'exit' para salir: ");
        string dpi;
        while ((dpi = Console.ReadLine()) != "exit")
        {
            // Codificar el DPI ingresado
            List<LZ77Token> encodedDpiTokens = encoder.Encode(dpi);

            // Convertir tokens a la cadena en formato deseado
            string customString = ConvertTokensToString(encodedDpiTokens);
            Console.WriteLine($"DPI Codificado: {customString}");

            // Convertir la lista de tokens a un formato que el árbol pueda usar para buscar
            string encodedDpiForSearch = JsonConvert.SerializeObject(encodedDpiTokens);

            Record user = bTree.SearchByDpi(encodedDpiForSearch);
            if (user != null)
            {
                // Decodificar el DPI antes de mostrarlo
                user.DPI = encoder.Decode(JsonConvert.DeserializeObject<List<LZ77Token>>(user.DPI));

                // Mostrar el usuario en formato JSON con indentación
                string userJson = JsonConvert.SerializeObject(user, Formatting.Indented);
                Console.WriteLine($"Registro encontrado:\n{userJson}");
            }
            else
            {
                Console.WriteLine($"Ningun registro encontrado con el DPI {dpi}");
            }
            Console.WriteLine("Ingresa otro DPI para buscar o escribe 'exit' para salir: ");
        }
    }*/

    /*private static void UserSearch()
    {
        LZ77 encoder = new LZ77();  // Instancia del codificador

        Console.WriteLine("Ingresa un DPI para buscar o escribe 'exit' para salir: ");
        string dpi;
        while ((dpi = Console.ReadLine()) != "exit")
        {
            // Codificar el DPI ingresado
            string encodedDpi = JsonConvert.SerializeObject(encoder.Encode(dpi));

            Record user = bTree.SearchByDpi(encodedDpi);
            if (user != null)
            {
                // Decodificar el DPI antes de mostrarlo
                user.DPI = encoder.Decode(JsonConvert.DeserializeObject<List<Tuple<int, int, char>>>(user.DPI));

                // Mostrar el usuario en formato JSON con indentación
                string userJson = JsonConvert.SerializeObject(user, Formatting.Indented);
                Console.WriteLine($"Registro encontrado:\n{userJson}");
            }
            else
            {
                Console.WriteLine($"Ningun registro encontrado con el DPI {dpi}");
            }
            Console.WriteLine("Ingresa otro DPI para buscar o escribe 'exit' para salir: ");
        }
    }*/
}

public class Record
{
    public string Name { get; set; }
    public string DPI { get; set; }
    public string DateBirth { get; set; }
    public string Address { get; set; }
    public List<string> Companies { get; set; }

    public Record()
    {
        Companies = new List<string>();
    }
}
public class BTreeNode
{
    public Record[] Records { get; set; }
    public int Degree { get; private set; }
    public BTreeNode[] Children { get; set; }
    public int RecordCount { get; set; }
    public bool IsLeaf { get; set; }

    public BTreeNode(int degree, bool isLeaf)
    {
        Degree = degree;
        IsLeaf = isLeaf;
        Records = new Record[2 * degree - 1];
        Children = new BTreeNode[2 * degree];
        RecordCount = 0;
    }
}
public class BTree
{
    private BTreeNode root;
    private int degree;

    public BTree(int degree)
    {
        root = null;
        this.degree = degree;
    }

    // Método para buscar un registro basado en el nombre
    public Record SearchByDpi(string encodedDpi)
    {
        return SearchByDpi(root, encodedDpi);
    }
    private Record SearchByDpi(BTreeNode node, string encodedDpi)
    {
        int idx = 0;

        // Encuentra el índice del primer registro mayor o igual que encodedDpi
        while (idx < node.RecordCount && encodedDpi.CompareTo(node.Records[idx].DPI) > 0)
            idx++;

        // Si el DPI es igual al registro en el índice idx, hemos encontrado el registro
        if (idx < node.RecordCount && node.Records[idx].DPI == encodedDpi)
            return node.Records[idx];

        // Si este nodo es un nodo hoja, entonces el DPI no está presente en el árbol
        if (node.IsLeaf)
            return null;

        // De lo contrario, busca el DPI en el subárbol correspondiente
        return SearchByDpi(node.Children[idx], encodedDpi);
    }

    // Método para insertar un nuevo registro
    public void Insert(Record user)
    {
        // Si el árbol está vacío
        if (root == null)
        {
            root = new BTreeNode(degree, true);
            root.Records[0] = user;
            root.RecordCount = 1;
        }
        else
        {
            // Si el nodo raíz está lleno, el árbol crece en altura
            if (root.RecordCount == 2 * degree - 1)
            {
                BTreeNode newNode = new BTreeNode(degree, false);
                newNode.Children[0] = root;
                SplitChild(newNode, 0, root);

                // Decide cuál de los dos hijos va a tener el nuevo registro.
                int i = 0;
                if (newNode.Records[0].DPI.CompareTo(user.DPI) < 0)  // Comparar DPI
                    i++;
                InsertNonFull(newNode.Children[i], user);

                root = newNode;
            }
            else
            {
                InsertNonFull(root, user);
            }
        }
    }

    // Método para eliminar un registro
    public void Delete(string dpi)
    {
        if (root == null)
        {
            Console.WriteLine("The tree is empty");
            return;
        }

        // Call the recursive delete function for root
        DeleteRecursive(root, dpi);

        // If the root node has 0 keys, make its first child the new root if it exists, otherwise set root as null.
        if (root.RecordCount == 0)
        {
            BTreeNode tempNode = root;

            if (root.IsLeaf)
                root = null;
            else
                root = root.Children[0];

            // Free the old root
            // In C#, garbage collector will take care of freeing, so we don't explicitly free the node.
        }

        return;
    }

    private void InsertNonFull(BTreeNode node, Record record)
    {
        int i = node.RecordCount - 1;

        // If this is a leaf node
        if (node.IsLeaf)
        {
            // Find the location of the new key in the records array and move other records to make space
            while (i >= 0 && record.DPI.CompareTo(node.Records[i].DPI) < 0)
            {
                node.Records[i + 1] = node.Records[i];
                i--;
            }

            // Insert the new record at the found location
            node.Records[i + 1] = record;
            node.RecordCount += 1;
        }
        else
        {
            // If this node is not a leaf node

            // Find the child which is going to have the new key
            while (i >= 0 && record.DPI.CompareTo(node.Records[i].DPI) < 0)
                i--;

            i++;
            BTreeNode child = node.Children[i];

            // If the child is full, then split it
            if (child.RecordCount == 2 * degree - 1)
            {
                SplitChild(node, i, child);

                // After the split, the middle record of child will move up to node, which will cause child to have degree-1 records.
                // Determine which of the two children will now receive the record (either the original child or its right sibling resulting from the split)
                if (record.DPI.CompareTo(node.Records[i].DPI) > 0)
                    i++;
            }

            InsertNonFull(node.Children[i], record);
        }
    }
    private void SplitChild(BTreeNode parentNode, int i, BTreeNode nodeToSplit)
    {
        BTreeNode newNode = new BTreeNode(nodeToSplit.Degree, nodeToSplit.IsLeaf);
        newNode.RecordCount = degree - 1;

        // Copy the last (degree-1) records of nodeToSplit to newNode
        for (int j = 0; j < degree - 1; j++)
            newNode.Records[j] = nodeToSplit.Records[j + degree];

        // If nodeToSplit is not a leaf, copy the last degree children of nodeToSplit to newNode
        if (!nodeToSplit.IsLeaf)
        {
            for (int j = 0; j < degree; j++)
                newNode.Children[j] = nodeToSplit.Children[j + degree];
        }

        // Reduce the number of records in nodeToSplit
        nodeToSplit.RecordCount = degree - 1;

        // Create space for a new child in the parentNode
        for (int j = parentNode.RecordCount; j >= i + 1; j--)
            parentNode.Children[j + 1] = parentNode.Children[j];

        // Link the newNode as child of parentNode
        parentNode.Children[i + 1] = newNode;

        // Move the middle record of nodeToSplit up to parentNode
        for (int j = parentNode.RecordCount - 1; j >= i; j--)
            parentNode.Records[j + 1] = parentNode.Records[j];

        parentNode.Records[i] = nodeToSplit.Records[degree - 1];

        // Increment count of records in parentNode
        parentNode.RecordCount += 1;
    }
    private void DeleteRecursive(BTreeNode node, string dpi)
    {
        int idx = 0;

        // Find the index of the record with the given name or the first record that is greater
        while (idx < node.RecordCount && dpi.CompareTo(node.Records[idx].DPI) > 0)
            idx++;

        // If the record to be deleted is found in the node
        if (idx < node.RecordCount && dpi == node.Records[idx].DPI)
        {
            if (node.IsLeaf)
            {
                // Simply remove the record from the leaf node
                RemoveFromLeaf(node, idx);
            }
            else
            {
                // Handle the more complex case of removing a record from a non-leaf node
                RemoveFromNonLeaf(node, idx);
            }
        }
        else
        {
            if (node.IsLeaf)
            {
                return;
            }

            // The flag indicates whether the record to be removed is present in the sub-tree rooted at node.Children[idx]
            bool flag = (idx == node.RecordCount);

            // If the child where the record is supposed to exist has less than degree records, fill that child
            if (node.Children[idx].RecordCount < degree)
                Fill(node, idx);

            // Recurse to the proper child after potential modifications
            if (flag && idx > node.RecordCount)
                DeleteRecursive(node.Children[idx - 1], dpi);
            else
                DeleteRecursive(node.Children[idx], dpi);
        }
    }
    private void RemoveFromLeaf(BTreeNode node, int idx)
    {
        for (int i = idx + 1; i < node.RecordCount; ++i)
            node.Records[i - 1] = node.Records[i];

        node.RecordCount--;
    }
    private void Fill(BTreeNode node, int idx)
    {
        if (idx != 0 && node.Children[idx - 1].RecordCount >= degree)
            BorrowFromPrev(node, idx);
        else if (idx != node.RecordCount && node.Children[idx + 1].RecordCount >= degree)
            BorrowFromNext(node, idx);
        else
        {
            if (idx != node.RecordCount)
                Merge(node, idx);
            else
                Merge(node, idx - 1);
        }
    }
    private void BorrowFromPrev(BTreeNode node, int idx)
    {
        BTreeNode child = node.Children[idx];
        BTreeNode sibling = node.Children[idx - 1];

        // Move all records of child one step ahead
        for (int i = child.RecordCount - 1; i >= 0; --i)
            child.Records[i + 1] = child.Records[i];

        // If child is not a leaf, move all child pointers one step ahead
        if (!child.IsLeaf)
        {
            for (int i = child.RecordCount; i >= 0; --i)
                child.Children[i + 1] = child.Children[i];
        }

        // Setting child's first record equal to node's idx-th record
        child.Records[0] = node.Records[idx - 1];

        // Moving the last child of the sibling as the first child of child
        if (!node.IsLeaf)
            child.Children[0] = sibling.Children[sibling.RecordCount];

        // Moving the last record from the sibling to the parent
        node.Records[idx - 1] = sibling.Records[sibling.RecordCount - 1];

        child.RecordCount += 1;
        sibling.RecordCount -= 1;

        return;
    }
    private void BorrowFromNext(BTreeNode node, int idx)
    {
        BTreeNode child = node.Children[idx];
        BTreeNode sibling = node.Children[idx + 1];

        // Setting the last record of child equal to idx-th record from node
        child.Records[child.RecordCount] = node.Records[idx];

        // Moving the first child of the sibling as the last child of child
        if (!child.IsLeaf)
            child.Children[child.RecordCount + 1] = sibling.Children[0];

        // Moving the first record from the sibling to idx-th position in node
        node.Records[idx] = sibling.Records[0];

        // Moving all records of sibling one step behind
        for (int i = 1; i < sibling.RecordCount; ++i)
            sibling.Records[i - 1] = sibling.Records[i];

        // Moving the child pointers of the sibling one step behind
        if (!sibling.IsLeaf)
        {
            for (int i = 1; i <= sibling.RecordCount; ++i)
                sibling.Children[i - 1] = sibling.Children[i];
        }

        child.RecordCount += 1;
        sibling.RecordCount -= 1;

        return;
    }
    private void Merge(BTreeNode node, int idx)
    {
        BTreeNode child = node.Children[idx];
        BTreeNode sibling = node.Children[idx + 1];

        child.Records[degree - 1] = node.Records[idx];

        for (int i = 0; i < sibling.RecordCount; ++i)
            child.Records[i + degree] = sibling.Records[i];

        if (!child.IsLeaf)
        {
            for (int i = 0; i <= sibling.RecordCount; ++i)
                child.Children[i + degree] = sibling.Children[i];
        }

        for (int i = idx + 1; i < node.RecordCount; ++i)
            node.Records[i - 1] = node.Records[i];

        for (int i = idx + 2; i <= node.RecordCount; ++i)
            node.Children[i - 1] = node.Children[i];

        child.RecordCount += sibling.RecordCount + 1;
        node.RecordCount--;

        // Liberar el nodo hermano
        // En C#, el recolector de basura se encargará de liberar, así que no liberamos explícitamente el nodo.
    }
    private void RemoveFromNonLeaf(BTreeNode node, int idx)
    {
        Record key = node.Records[idx];

        // Caso 1
        if (node.Children[idx].RecordCount >= degree)
        {
            Record pred = GetPredecessor(node, idx);
            node.Records[idx] = pred;
            DeleteRecursive(node.Children[idx], pred.Name);
        }
        // Caso 2
        else if (node.Children[idx + 1].RecordCount >= degree)
        {
            Record succ = GetSuccessor(node, idx);
            node.Records[idx] = succ;
            DeleteRecursive(node.Children[idx + 1], succ.Name);
        }
        // Caso 3
        else
        {
            Merge(node, idx);
            DeleteRecursive(node.Children[idx], key.Name);
        }
    }
    private Record GetPredecessor(BTreeNode node, int idx)
    {
        // Mueve al nodo más a la derecha hasta llegar a un nodo hoja
        BTreeNode cur = node.Children[idx];
        while (!cur.IsLeaf)
            cur = cur.Children[cur.RecordCount];

        // Devuelve el último registro del nodo hoja
        return cur.Records[cur.RecordCount - 1];
    }
    private Record GetSuccessor(BTreeNode node, int idx)
    {
        // Mueve al nodo más a la izquierda hasta llegar a un nodo hoja
        BTreeNode cur = node.Children[idx + 1];
        while (!cur.IsLeaf)
            cur = cur.Children[0];

        // Devuelve el primer registro del nodo hoja
        return cur.Records[0];
    }
}


// LZ77Token.cs
public record LZ77Token(int Distance, int Length, char NextChar)
{
    public override string ToString()
    {
        return $"<{Distance}, {Length}, {NextChar}>";
    }
}

public class LZ77
{
    public int WindowSize { get; }
    public int LookaheadBufferSize { get; }

    public LZ77(int windowSize = 4096, int lookaheadBufferSize = 32)
    {
        WindowSize = windowSize;
        LookaheadBufferSize = lookaheadBufferSize;
    }

    public List<LZ77Token> Encode(string input)
    {
        List<LZ77Token> output = new List<LZ77Token>();
        int i = 0;

        while (i < input.Length)
        {
            int maxMatchDistance = 0;
            int maxMatchLength = 0;
            char nextChar = '\0';

            for (int j = Math.Max(i - WindowSize, 0); j < i; j++)
            {
                int matchLength = 0;

                while ((i + matchLength) < input.Length &&
                       input[j + matchLength] == input[i + matchLength] &&
                       matchLength < LookaheadBufferSize)
                {
                    matchLength++;
                }

                if (matchLength > maxMatchLength)
                {
                    maxMatchDistance = i - j;
                    maxMatchLength = matchLength;
                }
            }

            if (i + maxMatchLength < input.Length)
            {
                nextChar = input[i + maxMatchLength];
            }

            // Agregar un nuevo LZ77Token en lugar de Tuple
            output.Add(new LZ77Token(maxMatchDistance, maxMatchLength, nextChar));
            i += maxMatchLength + 1;
        }

        return output;
    }

    public string Decode(List<LZ77Token> encodedData)
    {
        StringBuilder output = new StringBuilder();

        foreach (var token in encodedData)
        {
            int distance = token.Distance;
            int length = token.Length;

            for (int i = 0; i < length; i++)
            {
                output.Append(output[output.Length - distance]);
            }

            output.Append(token.NextChar);
        }

        return output.ToString();
    }
}