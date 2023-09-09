using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB01EDII_DMRA_1084522
{
    internal class Program
    {
        public class Data
        {
            public string name { get; set; }
            public long dpi { get; set; }
            public string datebirth { get; set; }
            public string address { get; set; }
        }
        public class AccionData
        {
            public string Accion { get; set; }

        }

        /**********************************************************************/
        public class AVLNode
        {
            //public string Name { get; set; }
            public long Dpi { get; set; }
            public int Height { get; set; }
            public AVLNode Left { get; set; }
            public AVLNode Right { get; set; }

            public AVLNode(long dpi)
            {
                //Name = name;
                Dpi = dpi;
                Height = 1;
                Left = null;
                Right = null;
            }
        }
        public class AVLTree
        {
            public AVLNode root;

            public AVLTree()
            {
                root = null;
            }

            // Función para obtener la altura de un nodo
            public int GetHeight(AVLNode node)
            {
                if (node == null)
                    return 0;
                return node.Height;
            }

            // Función para obtener el balance de un nodo
            public int GetBalance(AVLNode node)
            {
                if (node == null)
                    return 0;
                return GetHeight(node.Left) - GetHeight(node.Right);
            }

            // Función para rotar a la derecha un subárbol con raíz en y
            public AVLNode RotateRight(AVLNode y)
            {
                AVLNode x = y.Left;
                AVLNode T2 = x.Right;

                // Realizar la rotación
                x.Right = y;
                y.Left = T2;

                // Actualizar alturas
                y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;
                x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;

                return x;
            }

            // Función para rotar a la izquierda un subárbol con raíz en x
            public AVLNode RotateLeft(AVLNode x)
            {
                AVLNode y = x.Right;
                AVLNode T2 = y.Left;

                // Realizar la rotación
                y.Left = x;
                x.Right = T2;

                // Actualizar alturas
                x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;
                y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;

                return y;
            }

            // Función para insertar un valor DPI en el árbol AVL
            public void Insert(long dpi)
            {
                root = Insert(root, dpi);
            }

            public AVLNode Insert(AVLNode node, long dpi)
            {
                if (node == null)
                    return new AVLNode(dpi);

                if (dpi < node.Dpi)
                    node.Left = Insert(node.Left, dpi);
                else if (dpi > node.Dpi)
                    node.Right = Insert(node.Right, dpi);
                else // No permitir duplicados
                    return node;

                // Actualizar altura del nodo actual
                node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

                // Obtener el balance del nodo y verificar si está desequilibrado
                int balance = GetBalance(node);

                // Caso de desequilibrio izquierda-izquierda
                if (balance > 1 && dpi < node.Left.Dpi)
                    return RotateRight(node);

                // Caso de desequilibrio derecha-derecha
                if (balance < -1 && dpi > node.Right.Dpi)
                    return RotateLeft(node);

                // Caso de desequilibrio izquierda-derecha
                if (balance > 1 && dpi > node.Left.Dpi)
                {
                    node.Left = RotateLeft(node.Left);
                    return RotateRight(node);
                }

                // Caso de desequilibrio derecha-izquierda
                if (balance < -1 && dpi < node.Right.Dpi)
                {
                    node.Right = RotateRight(node.Right);
                    return RotateLeft(node);
                }

                return node;
            }
            /**********************************************************************/
            static void InsertRead()
            {
                string filePath = "C:\\Users\\driva\\OneDrive - Universidad Rafael Landivar\\Escritorio\\LAB01EDII_DMRA_1084522\\LAB01EDII_DMRA_1084522\\datos.txt"; // Cambia esto al nombre de tu archivo de texto

                // Verificamos si el archivo existe
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("El archivo no existe.");
                    return;
                }

                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                string[] lines = File.ReadAllLines(filePath);

                                // Dividimos la línea en campos utilizando punto y coma como delimitador
                                string[] campos = line.Split(';');

                                // Aseguramos que haya al menos dos campos (ACCION y DATA)
                                if (campos.Length >= 2)
                                {
                                    string accion = campos[0]; // El primer campo es ACCION
                                    string jsonData = campos[1]; // El segundo campo es DATA en formato JSON

                                    Data data = JsonConvert.DeserializeObject<Data>(jsonData);
                                    //Console.Write($"ACCION: {accion}, NAME: {data.name}, DPI: {data.dpi}, DATE BIRTH: {data.datebirth}, ADRESS: {data.address}\n");
                                    if (accion == "INSERT")
                                    {
                                        AVLTree tree = new AVLTree();
                                        // Insertar valores del DPI en el árbol
                                        tree.Insert(data.dpi);
                                        Console.WriteLine($" name: {data.name}, dpi: {data.dpi}, dateBirth: {data.datebirth}, address: {data.name}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("La línea no tiene la estructura esperada: " + line);
                                }
                            }
                        }
                        Console.ReadKey();
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Ocurrió un error al leer el archivo: " + ex.Message);
                }
            }
            static void Buscar()
            {
                string Sname;

                string filePath = "C:\\Users\\driva\\OneDrive - Universidad Rafael Landivar\\Escritorio\\LAB01EDII_DMRA_1084522\\LAB01EDII_DMRA_1084522\\datos.txt"; // Cambia esto al nombre de tu archivo de texto

                // Verificamos si el archivo existe
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("El archivo no existe.");
                    return;
                }

                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                string[] lines = File.ReadAllLines(filePath);

                                // Dividimos la línea en campos utilizando punto y coma como delimitador
                                string[] campos = line.Split(';');

                                // Aseguramos que haya al menos dos campos (ACCION y DATA)
                                if (campos.Length >= 2)
                                {
                                    string accion = campos[0]; // El primer campo es ACCION
                                    string jsonData = campos[1]; // El segundo campo es DATA en formato JSON

                                    Data data = JsonConvert.DeserializeObject<Data>(jsonData);
                                    //Console.Write($"ACCION: {accion}, NAME: {data.name}, DPI: {data.dpi}, DATE BIRTH: {data.datebirth}, ADRESS: {data.address}\n");

                                    Console.WriteLine("Ingrese el nombre y apellido a buscar");
                                    Sname = Console.ReadLine();
                                    if (Sname == data.name)
                                    {
                                        Console.Write($"NAME: {data.name}, DPI: {data.dpi}, DATE BIRTH: {data.datebirth}, ADRESS: {data.address}\n");
                                        Console.ReadKey();
                                    }
                                    else
                                    {
                                        Console.WriteLine("No hay nombres asignados");
                                        Console.ReadKey();

                                    }
                                    Console.Clear();
                                }
                                else
                                {
                                    Console.WriteLine("La línea no tiene la estructura esperada: " + line);
                                }
                            }
                        }
                        Console.ReadKey();
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Ocurrió un error al leer el archivo: " + ex.Message);
                }
            }
            static void PatchRead()
            {

            }

            static void DeleteRead()
            {

            }
            static void Main(string[] args)
            {

                int op = 0;
                Console.WriteLine("MENU");
                Console.WriteLine("1. INSERT");
                Console.WriteLine("2. DELETE");
                Console.WriteLine("3. PATCH");
                Console.WriteLine("4. SEARCH");
                op = Convert.ToInt16(Console.ReadLine());


                switch (op)
                {
                    case 1:
                        InsertRead();
                        break;
                    case 2:

                        break;
                    case 3:
                        break;
                    case 4:
                        Buscar();
                        break;
                }
            }

         
            
        }
    }
}


