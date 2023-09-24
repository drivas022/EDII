using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2_DiegoRivas
{
    public class PersonData
    {
        public string name { get; set; }
        public string dpi { get; set; }
        public string datebirth { get; set; }
        public string address { get; set; }
        public List<string> companies { get; set; }
        public string Action { get; set; } // Campo para rastrear la acción ("INSERT", "DELETE", "PATCH")
    }
    /****************************************************************************************/
    public class AVLNode
    {
        public PersonData Data { get; set; }
        public AVLNode Left { get; set; }
        public AVLNode Right { get; set; }
        public int Height { get; set; }
    }

    // Estructura básica de un árbol AVL
    public class AVLTree
    {
        private AVLNode Root { get; set; }

        // Métodos para insertar, eliminar, buscar y actualizar datos en el árbol...
        // Método auxiliar para obtener la altura de un nodo
        private int Height(AVLNode node)
        {
            return node?.Height ?? 0;
        }

        // Método auxiliar para obtener el balanceo de un nodo
        private int GetBalance(AVLNode node)
        {
            return node == null ? 0 : Height(node.Left) - Height(node.Right);
        }

        // Método auxiliar para rotar a la derecha
        private AVLNode RightRotate(AVLNode y)
        {
            AVLNode x = y.Left;
            AVLNode T3 = x.Right;

            x.Right = y;
            y.Left = T3;

            y.Height = Math.Max(Height(y.Left), Height(y.Right)) + 1;
            x.Height = Math.Max(Height(x.Left), Height(x.Right)) + 1;

            return x;
        }

        // Método auxiliar para rotar a la izquierda
        private AVLNode LeftRotate(AVLNode x)
        {
            AVLNode y = x.Right;
            AVLNode T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            x.Height = Math.Max(Height(x.Left), Height(x.Right)) + 1;
            y.Height = Math.Max(Height(y.Left), Height(y.Right)) + 1;

            return y;
        }

        public void Insert(PersonData data)
        {
            Root = Insert(Root, data);
        }

        private AVLNode Insert(AVLNode node, PersonData data)
        {
            // 1. Insert como un árbol binario de búsqueda
            if (node == null)
                return new AVLNode { Data = data, Height = 1 };

            if (data.dpi.CompareTo(node.Data.dpi) < 0)
                node.Left = Insert(node.Left, data);
            else if (data.dpi.CompareTo(node.Data.dpi) > 0)
                node.Right = Insert(node.Right, data);
            else
                return node; // No se permiten duplicados

            // 2. Actualizar la altura del nodo actual
            node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;

            // 3. Obtener el factor de balanceo para verificar si se desequilibró
            int balance = GetBalance(node);

            // Si está desequilibrado, hay 4 casos posibles
            // Left Left Case
            if (balance > 1 && data.dpi.CompareTo(node.Left.Data.dpi) < 0)
                return RightRotate(node);
            // Right Right Case
            if (balance < -1 && data.dpi.CompareTo(node.Right.Data.dpi) > 0)
                return LeftRotate(node);
            // Left Right Case
            if (balance > 1 && data.dpi.CompareTo(node.Left.Data.dpi) > 0)
            {
                node.Left = LeftRotate(node.Left);
                return RightRotate(node);
            }
            // Right Left Case
            if (balance < -1 && data.dpi.CompareTo(node.Right.Data.dpi) < 0)
            {
                node.Right = RightRotate(node.Right);
                return LeftRotate(node);
            }

            return node;
        }

        public void Delete(string dpi)
        {
            Root = Delete(Root, dpi);
        }

        private AVLNode Delete(AVLNode root, string dpi)
        {
            // 1. Eliminar como un árbol binario de búsqueda
            if (root == null) return root;

            if (dpi.CompareTo(root.Data.dpi) < 0)
                root.Left = Delete(root.Left, dpi);
            else if (dpi.CompareTo(root.Data.dpi) > 0)
                root.Right = Delete(root.Right, dpi);
            else
            {
                if (root.Left == null || root.Right == null)
                {
                    AVLNode temp = root.Left ?? root.Right;
                    root = temp;
                }
                else
                {
                    root.Data = MinValue(root.Right);
                    root.Right = Delete(root.Right, root.Data.dpi);
                }
            }

            if (root == null) return root;

            // 2. Actualizar la altura del nodo actual
            root.Height = Math.Max(Height(root.Left), Height(root.Right)) + 1;

            // 3. Obtener el factor de balanceo para verificar si se desequilibró
            int balance = GetBalance(root);

            // Si está desequilibrado, hay 4 casos posibles
            // Left Left Case
            if (balance > 1 && GetBalance(root.Left) >= 0)
                return RightRotate(root);
            // Left Right Case
            if (balance > 1 && GetBalance(root.Left) < 0)
            {
                root.Left = LeftRotate(root.Left);
                return RightRotate(root);
            }
            // Right Right Case
            if (balance < -1 && GetBalance(root.Right) <= 0)
                return LeftRotate(root);
            // Right Left Case
            if (balance < -1 && GetBalance(root.Right) > 0)
            {
                root.Right = RightRotate(root.Right);
                return LeftRotate(root);
            }

            return root;
        }

        private PersonData MinValue(AVLNode root)
        {
            AVLNode current = root;
            while (current.Left != null)
                current = current.Left;
            return current.Data;
        }

        public void Patch(PersonData data)
        {
            AVLNode node = Search(Root, data.dpi);
            if (node != null)
            {
                // Actualiza los valores del nodo encontrado
                node.Data.name = data.name;
                node.Data.datebirth = data.datebirth;
                node.Data.address = data.address;
                node.Data.companies = data.companies;
                node.Data.Action = "PATCH";
            }
            else
            {
                // Si no se encuentra el nodo, se inserta el nuevo dato en el árbol
                Insert(data);
            }
        }

        private AVLNode Search(AVLNode root, string dpi)
        {
            while (root != null)
            {
                if (dpi.CompareTo(root.Data.dpi) < 0)
                    root = root.Left;
                else if (dpi.CompareTo(root.Data.dpi) > 0)
                    root = root.Right;
                else
                    return root;
            }
            return null;
        }
        public PersonData SearchByName(string name)
        {
            return SearchByName(Root, name)?.Data;
        }

        private AVLNode SearchByName(AVLNode root, string name)
        {
            if (root == null) return null;

            if (name.CompareTo(root.Data.name) == 0)
                return root;
            else if (name.CompareTo(root.Data.name) < 0)
                return SearchByName(root.Left, name);
            else
                return SearchByName(root.Right, name);
        }

        // Actualizar los métodos para modificar el campo Action
        public void ActInsert(PersonData data)
        {
            data.Action = "INSERT";
            Root = Insert(Root, data);
        }

        public void ActDelete(string dpi)
        {
            var nodeToDelete = Search(Root, dpi);
            if (nodeToDelete != null)
            {
                nodeToDelete.Data.Action = "DELETE";
            }
            Root = Delete(Root, dpi);
        }

        public void ActPatch(PersonData data)
        {
            AVLNode node = Search(Root, data.dpi);
            if (node != null)
            {
                data.Action = "PATCH";
                node.Data = data;
            }
        }


    }//llave de la clase AVL tree








    /****************************************************************************************/
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\\Users\\driva\\OneDrive - Universidad Rafael Landivar\\Escritorio\\Lab2_DiegoRivas\\Lab2_DiegoRivas\\input.csv"; // Puedes cambiar esta dirección según lo necesites
            ProcessCSVFile(path);

            AVLTree tree = ProcessCSVFile(path);

            // Interacción con el usuario para buscar por nombre
            Console.WriteLine("Ingrese el nombre de la persona que desea buscar:");
            string nameToSearch = Console.ReadLine();

            var searchData = tree.SearchByName(nameToSearch);

            if (searchData != null)
            {
                Console.WriteLine($"Datos para {searchData.name}:");
                Console.WriteLine($"Nombre: {searchData.name}");
                Console.WriteLine($"DPI: {searchData.dpi}");
                Console.WriteLine($"Fecha de Nacimiento: {searchData.datebirth}");
                Console.WriteLine($"Dirección: {searchData.address}");
                Console.WriteLine($"Compañías: {string.Join(", ", searchData.companies)}");
                Console.WriteLine($"La última acción realizada en estos datos fue: {searchData.Action}");
            }
            else
            {
                Console.WriteLine("Datos no encontrados.");
            }
            Console.ReadKey();
        }
        public static AVLTree ProcessCSVFile(string filePath)
        {
            var tree = new AVLTree();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(new char[] { ';' }, 2);

                    if (values.Length < 2)
                        continue;

                    var action = values[0];
                    var jsonData = values[1];
                    var personData = JsonConvert.DeserializeObject<PersonData>(jsonData);

                    switch (action)
                    {
                        case "INSERT":
                            tree.Insert(personData);
                            break;
                        case "DELETE":
                            tree.Delete(personData.dpi);
                            break;
                        case "PATCH":
                            tree.Patch(personData);
                            break;
                    }
                }
            }

            return tree;
        }

        /*public static void ProcessCSVFile(string filePath)
        {
            var tree = new AVLTree();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(new char[] { ';' }, 2);

                    if (values.Length < 2)
                        continue;

                    var action = values[0];
                    var jsonData = values[1];
                    var personData = JsonConvert.DeserializeObject<PersonData>(jsonData);

                    switch (action)
                    {
                        case "INSERT":
                            tree.Insert(personData);
                            break;
                        case "DELETE":
                            tree.Delete(personData.dpi); // Asumimos que el dpi es único
                            break;
                        case "PATCH":
                            tree.Patch(personData); // Este método buscaría el nodo usando dpi y actualizaría los datos
                            break;
                    }
                }
            }
        }*/
    }

    
}
