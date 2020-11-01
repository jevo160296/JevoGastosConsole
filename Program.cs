using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JevoGastosCore;
using JevoGastosCore.Model;
using JevoGastosCore.ModelView;
using JevoGastosCore.ModelView.EtiquetaMV;

namespace JevoGastosConsole
{
    class Program
    {
        internal class Data
        {
            public GastosContainer DbContainer { get; set; }
            public Data(GastosContainer dbcontext)
            {
                this.DbContainer = dbcontext;
            }
        }
        private static int Salir
        {
            get
            {
                return 4;
            }
        }
        private static Dictionary<int, string> Menu
        {
            get
            {
                return new Dictionary<int, string>
                {
                    [1] = "Agregar etiqueta",
                    [2] = "Agregar transaccion",
                    [3] = "Mostrar etiquetas",
                    [Salir] = "Salir"
                };
            }
        }
        private static Dictionary<int,string> MenuAddEtiqueta
        {
            get
            {
                return new Dictionary<int, string>
                {
                    [1]="Agregar ingreso",
                    [2]="Agregar cuenta",
                    [3]="Agregar gasto"
                };
            }
        }
        static void Main(string[] args)
        {
            int? selected;
            GastosContainer dbContainer = new GastosContainer(AppContext.BaseDirectory);
            Data data = new Data(dbContainer);
            do
            {
                selected = GenerateMenu<string>(Menu);
                MenuPrincipal(selected, data);
            } while (selected!=Salir);
            IngresoDAO iDAO = new IngresoDAO(dbContainer);
            
        }
        static void MenuPrincipal(int? selection,Data data)
        {
            switch (selection)
            {
                case 1:
                    AddEtiqueta(data);
                    break;
                case 2:
                    AddTransaccion(data);
                    break;
                case 3:
                    SeeEtiquetas(data);
                    break;
                default:
                    break;
            }
        }
        static void AddEtiqueta(Data data)
        {
            int? selection;
            string name;
            selection=GenerateMenu<string>(MenuAddEtiqueta);
            if (!(selection is null))
            {
                Console.WriteLine($"{MenuAddEtiqueta[(int)selection]} seleccionado");
                Console.WriteLine("Nombre: ");
                name = Console.ReadLine();
                switch (selection)
                {
                    //Ingreso
                    case 1:
                        data.DbContainer.IngresoDAO.Add(name);
                        break;
                    //Cuenta
                    case 2:
                        data.DbContainer.CuentaDAO.Add(name);
                        break;
                    //Gasto
                    case 3:
                        data.DbContainer.GastoDAO.Add(name);
                        break;
                    default:
                        break;
                }
            }
        }
        static void AddTransaccion(Data data)
        {
            Console.WriteLine("Agregando Transacción: ");
            ObservableCollection<Etiqueta> origenes =null, destinos=null;
            Etiqueta origen, destino;
            int? seleccion,origSelec,desSelec;
            double valor;
            string descripcion;
            Dictionary<int, string> transOptions = new Dictionary<int, string>
            {
                [1] = "Entrada",
                [2] = "Movimiento",
                [3] = "Salida"
            };
            seleccion = GenerateMenu<string>(transOptions);
            if (!(seleccion is null))
            {
                Console.WriteLine($"Agregando {transOptions[(int)seleccion].ToLower()}");
                switch (seleccion)
                {
                    case 1:
                        origenes = GetEtiquetas(data);//<Ingreso>(data);
                        destinos = GetEtiquetas(data);//< Cuenta>(data);
                        break;
                    case 2:
                        origenes = GetEtiquetas(data);//<Cuenta>(data);
                        destinos = GetEtiquetas(data);//<Cuenta>(data);
                        break;
                    case 3:
                        origenes = GetEtiquetas(data);//<Cuenta>(data);
                        destinos = GetEtiquetas(data);//<Gasto>(data);
                        break;
                    default:
                        break;
                }
                Console.WriteLine("Seleccionar origen: ");
                origSelec=GenerateMenu(origenes);
                if (!(origSelec is null))
                {
                    Console.WriteLine("Seleccionar destino: ");
                    desSelec = GenerateMenu(destinos);
                    if (!(desSelec is null))
                    {
                        origen = origenes[(int)origSelec - 1];
                        destino = destinos[(int)desSelec - 1];
                        Console.WriteLine("Valor: ");
                        if (!Double.TryParse(Console.ReadLine(), out valor))
                        {
                            valor = 0;
                        }
                        Console.WriteLine("Descripción: ");
                        descripcion = Console.ReadLine();
                        Console.WriteLine($"Agregando una transacción con la siguiente descripción {descripcion} \n Desde {origen} hasta {destino} \n Con un valor de {valor}.");
                        data.DbContainer.TransaccionDAO.Transaccion(origen, destino, valor, descripcion);
                    }
                }
            }
        }
        static ObservableCollection<Etiqueta> GetEtiquetas(Data data)
        {
            var c = data.DbContainer.EtiquetaDAO.Get();
            return c;
        }
        static void SeeEtiquetas(Data data)
        {
            ObservableCollection<Etiqueta> etiquetas;
            
            etiquetas = GetEtiquetas(data);
            int i = 0,max=100;
            foreach (Etiqueta item in etiquetas)
            {
                Console.WriteLine($"{item.ToString()}");
                i++;
                if (i>max)
                {
                    Console.WriteLine("...");
                    break;
                }
            }
            Console.WriteLine("Presiona cualquier tecla para continuar");
            Console.ReadKey(true);
        }
        static int GenerateMenu<T>(IEnumerable<T> options)
        {
            string[] soptions = new string[options.Count()];
            int i = 0;
            foreach (T option in options)
            {
                soptions[i] = option.ToString();
                i++;
            }
            return GenerateMenu(soptions);
        }
        static int GenerateMenu(string[] options)
        {
            int selected=-1,i=1;
            string cancel = "Cancelar";
            foreach (string option in options)
            {
                Console.WriteLine($"{i}. {option}");
                i++;
            }
            Console.WriteLine($"{i}. {cancel}");
            while (selected<1 || selected>options.Length+1)
            {
                selected = Console.ReadKey(true).KeyChar - 48;
            }
            if (selected==options.Length+1)
            {
                selected = -1;
            }
            return selected;
        }
        static int? GenerateMenu<T>(Dictionary<int,T> options)
        {
            int? selected;
            int indice;
            int cancel=1;
            IEnumerable<int> optionKeys = options.Keys;
            IEnumerable<T> optionNames = options.Values;
            bool continuar;

            foreach (int key in optionKeys)
            {
                Console.WriteLine($"{key}. {options[key].ToString()}");
            }
            while (optionKeys.Contains(cancel))
            {
                cancel++;
            }
            Console.WriteLine($"{cancel}. Cancelar");
            do
            {
                indice = Console.ReadKey(true).KeyChar - 48;
                if (!optionKeys.Contains(indice))
                {
                    selected = null;
                    if (indice==cancel)
                    {
                        continuar = false;
                    }
                    else
                    {
                        continuar = true;
                    }
                }
                else
                {
                    selected = indice;
                    continuar = false;
                }
            } while (continuar);
            return selected;
        }
    }
}