using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LAB01EDII_DMRA_1084522
{
    internal class Program
    {
        private static BTree bTree = new BTree(3);  // Asumimos un grado t=3 para el árbol B
        /*********************MAIN*******************/
        static void Main()
        {
            LoadCommandsFromFile(@"C:\Users\driva\OneDrive - Universidad Rafael Landivar\Escritorio\Lab_DiegoRivas_DatosII\Lab_DiegoRivas_DatosII\datos .txt");  // Reemplazar con la ruta correcta al archivo de comandos

            // Permitir que el usuario busque registros
            UserSearch();

        }
        /********************************************/
        private static void LoadCommandsFromFile(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("INSERT;"))
                    {
                        string json = line.Substring(7);
                        Record record = JsonConvert.DeserializeObject<Record>(json);
                        bTree.Insert(record);
                    }
                    else if (line.StartsWith("PATCH;"))
                    {
                        string json = line.Substring(6);
                        Record newRecord = JsonConvert.DeserializeObject<Record>(json);
                        Record existingRecord = bTree.Search(newRecord.Name);
                        if (existingRecord != null)
                        {
                            // Actualizar los campos del registro existente (aquí asumimos que todos los campos pueden ser actualizados)
                            existingRecord.DPI = newRecord.DPI;
                            existingRecord.DateBirth = newRecord.DateBirth;
                            existingRecord.Address = newRecord.Address;
                        }
                        else
                        {
                            Console.WriteLine($"No record found with name {newRecord.Name} for updating.");
                        }
                    }
                    else if (line.StartsWith("DELETE;"))
                    {
                        string json = line.Substring(7);
                        Record record = JsonConvert.DeserializeObject<Record>(json);
                        bTree.Delete(record.Name);
                    }
                }
            }
        }
        private static void UserSearch()
        {
            Console.WriteLine("Enter a name to search for (or type 'exit' to quit): ");
            string name;
            while ((name = Console.ReadLine()) != "exit")
            {
                Record record = bTree.Search(name);
                if (record != null)
                {
                    Console.WriteLine($"Found record: {JsonConvert.SerializeObject(record)}");
                }
                else
                {
                    Console.WriteLine($"No record found with name {name}");
                }
                Console.WriteLine("Enter another name to search for (or type 'exit' to quit): ");
            }
        }
    }
}

public class Record
{
    public string Name { get; set; }
    public string DPI { get; set; }
    public string DateBirth { get; set; }
    public string Address { get; set; }

    public Record(string name, string dpi, string dateBirth, string address)
    {
        Name = name;
        DPI = dpi;
        DateBirth = dateBirth;
        Address = address;
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
    public Record Search(string name)
    {
        return Search(root, name);
    }
    private Record Search(BTreeNode node, string name)
    {
        int idx = 0;

        // Encuentra el índice del primer registro mayor o igual que name
        while (idx < node.RecordCount && name.CompareTo(node.Records[idx].Name) > 0)
            idx++;

        // Si la clave es igual al registro en el índice idx, hemos encontrado el registro
        if (idx < node.RecordCount && node.Records[idx].Name == name)
            return node.Records[idx];

        // Si este nodo es un nodo hoja, entonces la clave no está presente en el árbol
        if (node.IsLeaf)
            return null;

        // De lo contrario, busca la clave en el subárbol correspondiente
        return Search(node.Children[idx], name);
    }
    // Método para insertar un nuevo registro
    public void Insert(Record record)
    {
        // 1. If the tree is empty
        if (root == null)
        {
            root = new BTreeNode(degree, true);
            root.Records[0] = record;
            root.RecordCount = 1;
        }
        else
        {
            // 2. If the root node is full, then tree grows in height
            if (root.RecordCount == 2 * degree - 1)
            {
                BTreeNode newNode = new BTreeNode(degree, false);
                newNode.Children[0] = root;
                SplitChild(newNode, 0, root);

                // Decide which of the two children is going to have the new record
                int i = 0;
                if (newNode.Records[0].Name.CompareTo(record.Name) < 0)
                    i++;
                InsertNonFull(newNode.Children[i], record);

                root = newNode;
            }
            else
            {
                InsertNonFull(root, record);
            }
        }
    }
    public void Delete(string name)
    {
        if (root == null)
        {
            Console.WriteLine("The tree is empty");
            return;
        }

        // Call the recursive delete function for root
        DeleteRecursive(root, name);

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

        if (node.IsLeaf)
        {
            while (i >= 0 && record.Name.CompareTo(node.Records[i].Name) < 0)
            {
                node.Records[i + 1] = node.Records[i];
                i--;
            }

            node.Records[i + 1] = record;
            node.RecordCount += 1;
        }
        else
        {
            while (i >= 0 && record.Name.CompareTo(node.Records[i].Name) < 0)
                i--;

            i++;
            BTreeNode child = node.Children[i];

            if (child.RecordCount == 2 * degree - 1)
            {
                SplitChild(node, i, child);

                if (record.Name.CompareTo(node.Records[i].Name) > 0)
                    i++;
            }

            InsertNonFull(node.Children[i], record);
        }
    }
    private void SplitChild(BTreeNode parentNode, int i, BTreeNode nodeToSplit)
    {
        BTreeNode newNode = new BTreeNode(nodeToSplit.Degree, nodeToSplit.IsLeaf);
        newNode.RecordCount = degree - 1;

        for (int j = 0; j < degree - 1; j++)
            newNode.Records[j] = nodeToSplit.Records[j + degree];

        if (!nodeToSplit.IsLeaf)
        {
            for (int j = 0; j < degree; j++)
                newNode.Children[j] = nodeToSplit.Children[j + degree];
        }

        nodeToSplit.RecordCount = degree - 1;

        for (int j = parentNode.RecordCount; j >= i + 1; j--)
            parentNode.Children[j + 1] = parentNode.Children[j];

        parentNode.Children[i + 1] = newNode;

        for (int j = parentNode.RecordCount - 1; j >= i; j--)
            parentNode.Records[j + 1] = parentNode.Records[j];

        parentNode.Records[i] = nodeToSplit.Records[degree - 1];

        parentNode.RecordCount += 1;
    }
    private void DeleteRecursive(BTreeNode node, string name)
    {
        int idx = 0;

        while (idx < node.RecordCount && name.CompareTo(node.Records[idx].Name) > 0)
            idx++;

        if (idx < node.RecordCount && name == node.Records[idx].Name)
        {
            if (node.IsLeaf)
            {
                RemoveFromLeaf(node, idx);
            }
            else
            {
                RemoveFromNonLeaf(node, idx);
            }
        }
        else
        {
            if (node.IsLeaf)
            {
                Console.WriteLine("The record with name " + name + " does not exist in the tree.");
                return;
            }

            bool flag = (idx == node.RecordCount);

            if (node.Children[idx].RecordCount < degree)
                Fill(node, idx);

            if (flag && idx > node.RecordCount)
                DeleteRecursive(node.Children[idx - 1], name);
            else
                DeleteRecursive(node.Children[idx], name);
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

        for (int i = child.RecordCount - 1; i >= 0; --i)
            child.Records[i + 1] = child.Records[i];

        if (!child.IsLeaf)
        {
            for (int i = child.RecordCount; i >= 0; --i)
                child.Children[i + 1] = child.Children[i];
        }

        child.Records[0] = node.Records[idx - 1];

        if (!node.IsLeaf)
            child.Children[0] = sibling.Children[sibling.RecordCount];

        node.Records[idx - 1] = sibling.Records[sibling.RecordCount - 1];

        child.RecordCount += 1;
        sibling.RecordCount -= 1;

        return;
    }
    private void BorrowFromNext(BTreeNode node, int idx)
    {
        BTreeNode child = node.Children[idx];
        BTreeNode sibling = node.Children[idx + 1];

        child.Records[child.RecordCount] = node.Records[idx];

        if (!child.IsLeaf)
            child.Children[child.RecordCount + 1] = sibling.Children[0];

        node.Records[idx] = sibling.Records[0];

        for (int i = 1; i < sibling.RecordCount; ++i)
            sibling.Records[i - 1] = sibling.Records[i];

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

