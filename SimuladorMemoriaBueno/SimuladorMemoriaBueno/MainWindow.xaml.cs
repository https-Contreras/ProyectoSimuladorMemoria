using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimuladorMemoriaBueno
{
    public partial class MainWindow : Window
    {
        private List<Slot> memoriaSlots = new List<Slot>(); // Lista de slots de memoria
        private ObservableCollection<Proceso> listaProcesos = new ObservableCollection<Proceso>(); // Lista de procesos
        private bool procesosAsignados = false; // Variable para verificar si los procesos han sido asignados
        private int contadorProcesos = 1; // Inicializa el contador para los procesos
        private bool pausa = false; // Indica si la simulación está en pausa
        private CancellationTokenSource cancellationTokenSource; // Controla las tareas en ejecución
        private ObservableCollection<Proceso> procesosEnMemoria = new ObservableCollection<Proceso>();


        public MainWindow()
        {
            InitializeComponent();
        }

        // Evento al hacer clic en "Aceptar"
        private void Button_Aceptar_Click(object sender, RoutedEventArgs e)
        {
            int numSlots = int.Parse(textBoxSlots.Text); // Leer cantidad de slots
            CrearMemoria(numSlots); // Crear memoria con slots aleatorios
            MostrarMemoria(); // Mostrar memoria en StackPanel
        }

        // Método para crear memoria con slots aleatorios
        private void CrearMemoria(int numSlots)
        {
            Random random = new Random();
            memoriaSlots.Clear();

            int totalMemoria = 10000; // Tamaño total de la memoria
            int memoriaRestante = totalMemoria;

            for (int i = 0; i < numSlots; i++)
            {
                int slotSize = random.Next(1, memoriaRestante / (numSlots - i)); // Tamaño aleatorio para cada slot
                memoriaSlots.Add(new Slot { Tamaño = slotSize, SlotId = i });
                memoriaRestante -= slotSize;
            }
        }

        // Método para mostrar la memoria en el StackPanel
        // Método para mostrar la memoria en el StackPanel
        // Método para mostrar la memoria en el StackPanel
        // Método para mostrar la memoria en el StackPanel con recuadros del mismo tamaño
        // Método para mostrar la memoria en el StackPanel con diseño atractivo
        private void MostrarMemoria()
        {
            panelMemoria.Children.Clear(); // Limpiar memoria visual

            foreach (var slot in memoriaSlots)
            {
                // Contenedor para un slot de memoria
                var stackSlot = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };

                // Cuadro visual que representa el tamaño del slot
                var cuadro = new Border
                {
                    Width = 150, // Tamaño fijo para los recuadros
                    Height = 50,
                    Background = slot.Asignado ? Brushes.Red : Brushes.White, // Rojo si está ocupado, blanco si está libre
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(5),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Colors.Gray,
                        Direction = 320,
                        ShadowDepth = 4,
                        Opacity = 0.6
                    }
                };

                // Etiqueta para número de slot y tamaño
                var textoInformacion = new TextBlock
                {
                    Text = slot.Asignado
                        ? $"Slot {slot.SlotId} - {slot.Tamaño} MB\n{slot.Proceso.Name} - Tiempo: {slot.Proceso.Duration}s"
                        : $"Slot {slot.SlotId} - {slot.Tamaño} MB",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    FontSize = slot.Asignado ? 16 : 14, // Más grande si está asignado
                    FontWeight = FontWeights.Bold, // Negrita
                    Foreground = slot.Asignado ? Brushes.White : Brushes.Black, // Blanco para contraste si está ocupado
                    TextWrapping = TextWrapping.Wrap
                };

                // Agregar elementos al contenedor del slot
                stackSlot.Children.Add(cuadro);
                stackSlot.Children.Add(textoInformacion);

                // Agregar el contenedor al StackPanel principal
                panelMemoria.Children.Add(stackSlot);
            }
        }




        // Evento al hacer clic en "Iniciar"






        private void TextBox_GotFocusSlots(object sender, RoutedEventArgs e)
        {
            // Si el texto es el predeterminado, lo eliminamos
            if (textBoxSlots.Text == "Ingrese cantidad de slots")
            {
                textBoxSlots.Text = ""; // Limpiamos el TextBox

            }
        }

        // Este evento se activa cuando el TextBox pierde el enfoque
        private void TextBox_LostFocusSlots(object sender, RoutedEventArgs e)
        {
            // Si el TextBox está vacío, restauramos el texto predeterminado
            if (string.IsNullOrWhiteSpace(textBoxSlots.Text))
            {
                textBoxSlots.Text = "Ingrese cantidad de slots"; // Establecemos el texto predeterminado

            }
        }


        // Método para crear 15 procesos aleatorios
        private async Task CrearProcesosYAsignar(string tipoAjuste, CancellationToken token)
        {
            Random random = new Random();

            while (!token.IsCancellationRequested)
            {
                var proceso = new Proceso
                {
                    ProcesoId = listaProcesos.Count + 1,
                    Name = $"Proceso {contadorProcesos}",
                    Size = random.Next(50, 500),
                    Duration = random.Next(10, 17),
                     Color = GenerarColorAleatorio()
                };

                contadorProcesos++;
                listaProcesos.Add(proceso);

                Dispatcher.Invoke(ActualizarListBoxProcesos);
                AsignarProcesos(tipoAjuste);

                await Task.Delay(2000, token);
            }
        }




        private async Task EliminarProcesos(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                for (int i = procesosEnMemoria.Count - 1; i >= 0; i--)
                {
                    var proceso = procesosEnMemoria[i];
                    proceso.Duration--;

                    if (proceso.Duration <= 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            // Eliminar de la lista de memoria
                            procesosEnMemoria.Remove(proceso);
                            ActualizarListBoxMemoria();

                            // Liberar el slot en el StackPanel
                            Slot slotLiberado = memoriaSlots.FirstOrDefault(slot => slot.Proceso == proceso);
                            if (slotLiberado != null)
                            {
                                slotLiberado.Asignado = false;
                                slotLiberado.Proceso = null;
                                MostrarMemoria(); // Reflejar el cambio en el StackPanel
                            }
                        });

                        await Task.Delay(3000); // Retraso antes de asignar otro proceso
                    }
                }

                await Task.Delay(1000); // Intervalo para verificar los procesos
            }
        }


        private void ActualizarListBoxMemoria()
        {
            listaBoxMemoria.ItemsSource = null;
            listaBoxMemoria.ItemsSource = procesosEnMemoria;
        }


        private void CrearProcesoNuevo()
        {
            Random random = new Random();

            // Crear un nuevo proceso con un tamaño y duración aleatoria
            var proceso = new Proceso
            {
                ProcesoId = listaProcesos.Count + 1, // Incrementa el ID con base en el número de procesos
                Name = $"Proceso {contadorProcesos}",
                Size = random.Next(50, 500), // Tamaño aleatorio en MB
                Duration = random.Next(10, 17) // Duración aleatoria en segundos
            };

            // Incrementar el contador para el siguiente proceso
            contadorProcesos++;

            // Agregar el nuevo proceso a la lista de procesos
            listaProcesos.Add(proceso);

            // Asignar el proceso a la memoria usando el tipo de ajuste seleccionado
            string tipoAjuste = ((ComboBoxItem)comboBoxAjuste.SelectedItem).Content.ToString();
            AsignarProcesos(tipoAjuste);

            // Actualizar la ListBox después de agregar el proceso
            Dispatcher.Invoke(() =>
            {
                ActualizarListBoxProcesos();
            });
        }


        // Método para actualizar la ListBox con los procesos creados
        private void ActualizarListBoxProcesos()
        {
            listaBoxProcesos.Items.Clear(); // Limpiar la ListBox de espera

            foreach (var proceso in listaProcesos)
            {
                var item = new ListBoxItem
                {
                    Content = $"{proceso.Name} - {proceso.Size} MB - {proceso.Duration}s",
                    Background = proceso.Color,
                    Foreground = Brushes.White // Texto blanco para contraste
                };

                listaBoxProcesos.Items.Add(item); // Añadir el proceso a la ListBox
            }
        }


        private Slot AsignarAjustePrimero(Proceso proceso)
        {
            return memoriaSlots.FirstOrDefault(slot => !slot.Asignado && slot.Tamaño >= proceso.Size);
        }


        private Slot AsignarAjusteMejor(Proceso proceso)
        {
            return memoriaSlots.Where(slot => !slot.Asignado && slot.Tamaño >= proceso.Size)
                               .OrderBy(slot => slot.Tamaño) // Ordena por tamaño
                               .FirstOrDefault();
        }


        private Slot AsignarAjustePeor(Proceso proceso)
        {
            return memoriaSlots.Where(slot => !slot.Asignado && slot.Tamaño >= proceso.Size)
                               .OrderByDescending(slot => slot.Tamaño) // Ordena de mayor a menor tamaño
                               .FirstOrDefault();
        }


        private void AsignarProcesos(string tipoAjuste)
        {
            foreach (var proceso in listaProcesos.ToList()) // Copia para evitar problemas al modificar
            {
                Slot slotAsignado = null;

                switch (tipoAjuste)
                {
                    case "Ajuste Primero":
                        slotAsignado = AsignarAjustePrimero(proceso);
                        break;
                    case "Ajuste Mejor":
                        slotAsignado = AsignarAjusteMejor(proceso);
                        break;
                    case "Ajuste Peor":
                        slotAsignado = AsignarAjustePeor(proceso);
                        break;
                }

                if (slotAsignado != null)
                {
                    slotAsignado.Asignado = true;
                    slotAsignado.Proceso = proceso;

                    // Mover proceso de la lista de espera a la memoria
                    Dispatcher.Invoke(() =>
                    {
                        listaProcesos.Remove(proceso); // Eliminar de la lista de espera
                        procesosEnMemoria.Add(proceso); // Agregar a la nueva ListBox
                        ActualizarListBoxProcesos(); // Actualizar la lista de espera
                        ActualizarListBoxMemoria(); // Actualizar la lista de procesos en memoria
                        MostrarMemoria(); // Actualizar el StackPanel de memoria
                    });
                }
            }
        }





        // Evento al hacer clic en "Iniciar"
        private async void Button_Iniciar_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxAjuste.SelectedItem != null)
            {
                pausa = false;
                cancellationTokenSource = new CancellationTokenSource();

                string tipoAjuste = ((ComboBoxItem)comboBoxAjuste.SelectedItem).Content.ToString();
                try
                {
                    var token = cancellationTokenSource.Token;
                    await Task.WhenAll(CrearProcesosYAsignar(tipoAjuste, token), EliminarProcesos(token));
                }
                catch (OperationCanceledException)
                {
                    // Manejar la cancelación si es necesario
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un tipo de ajuste.");
            }
        }


        private void Button_Reiniciar_Click(object sender, RoutedEventArgs e)
        {
            // Detener cualquier proceso en ejecución
            pausa = true; // Establecer el estado en pausa
            cancellationTokenSource?.Cancel(); // Cancelar cualquier tarea en ejecución

            // Limpiar la memoria
            memoriaSlots.Clear();
            panelMemoria.Children.Clear();

            // Limpiar la lista de procesos en espera
            listaProcesos.Clear();
            listaBoxProcesos.Items.Clear();

            // Limpiar la lista de procesos en ejecución
            procesosEnMemoria.Clear();
            

            // Reiniciar configuraciones
            comboBoxAjuste.SelectedIndex = -1; // Deseleccionar ajuste
            contadorProcesos = 1; // Reiniciar el contador de procesos

            // Mostrar mensaje de reinicio
            MessageBox.Show("Simulación reiniciada. Puede configurar nuevamente los parámetros.", "Reinicio completado", MessageBoxButton.OK, MessageBoxImage.Information);
        }




        private void Button_Pausar_Click(object sender, RoutedEventArgs e)
        {
            pausa = true; // Activar estado de pausa
            cancellationTokenSource?.Cancel(); // Detener tareas activas
            CambiarColorProcesosEnMemoria(Brushes.Yellow); // Cambiar el color de los procesos a amarillo
        }

        private async void Button_Reanudar_Click(object sender, RoutedEventArgs e)
        {
            if (!pausa) return;

            pausa = false; // Desactivar estado de pausa
            cancellationTokenSource = new CancellationTokenSource(); // Reiniciar el token de cancelación
            CambiarColorProcesosEnMemoria(Brushes.LightGreen); // Cambiar el color de los procesos a verde

            string tipoAjuste = ((ComboBoxItem)comboBoxAjuste.SelectedItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(tipoAjuste))
            {
                try
                {
                    // Reanudar las tareas de creación y eliminación de procesos
                    var token = cancellationTokenSource.Token;
                    await Task.WhenAll(CrearProcesosYAsignar(tipoAjuste, token), EliminarProcesos(token));
                }
                catch (OperationCanceledException)
                {
                    // Manejar la cancelación si es necesario
                }
            }
        }


        private void CambiarColorProcesosEnMemoria(Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var child in panelMemoria.Children)
                {
                    if (child is StackPanel stackPanel && stackPanel.Children[0] is Border border)
                    {
                        var slot = memoriaSlots.FirstOrDefault(s => stackPanel.Children.OfType<TextBlock>().Any(tb => tb.Text.Contains($"Slot {s.SlotId}")));
                        if (slot?.Asignado == true)
                        {
                            border.Background = color;
                        }
                    }
                }
            });
        }


        private Brush GenerarColorAleatorio()
        {
            Random random = new Random();
            byte r = (byte)random.Next(0, 256);
            byte g = (byte)random.Next(0, 256);
            byte b = (byte)random.Next(0, 256);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }


    }

    // Clase Slot para representar cada slot de memoria
    public class Slot
    {
        public int SlotId { get; set; }
        public int Tamaño { get; set; }
        public bool Asignado { get; set; } // Indica si el slot está asignado a un proceso
        public Proceso Proceso { get; set; } // Proceso asignado al slot
    }


    // Clase Proceso para representar cada proceso
    public class Proceso
    {
        public int ProcesoId { get; set; }
        public string Name { get; set; }
        public int Size { get; set; } // Tamaño en MB
        public int Duration { get; set; } // Duración en segundos
        public Brush Color { get; set; } // Color para la ListBox
    }

}
