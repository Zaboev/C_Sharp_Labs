using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        int maxPatients = 5; 
        int doctorCount = 3; 
        int maxConsultationTime = 10; 

        
        InfectiousDiseaseWard ward = new InfectiousDiseaseWard(maxPatients, doctorCount, maxConsultationTime);

        
        await ward.RunSimulation();
    }
}


class Patient
{
    private static int _idCounter = 0; 
    public int Id { get; } 
    public bool IsSick { get; set; } 
    public DateTime ArrivalTime { get; set; } 

    
    public Patient(bool isSick)
    {
        Id = Interlocked.Increment(ref _idCounter); 
        IsSick = isSick; 
        ArrivalTime = DateTime.Now; 
    }
}

class Doctor
{
    private static int _idCounter = 0; 
    private static Random _rand = new Random(); 

    public int Id { get; } 

    
    public Doctor()
    {
        Id = Interlocked.Increment(ref _idCounter); 
    }

    
    public async Task ConsultPatient(Patient patient, int maxConsultationTime)
    {
        
        int consultationTime = _rand.Next(1, maxConsultationTime + 1);

        
        string consultStartMessage = $"Доктор #{Id} начинает консультацию пациента #{patient.Id} ({(patient.IsSick ? "больного" : "здорового")}). Ожидаемое время: {consultationTime} единиц.";
        Console.WriteLine(consultStartMessage);
        await LogEventAsync(consultStartMessage); 

       
        await Task.Delay(consultationTime * 1000); 

        
        string consultEndMessage = $"Доктор #{Id} завершил консультацию пациента #{patient.Id} ({(patient.IsSick ? "больного" : "здорового")}).";
        Console.WriteLine(consultEndMessage);
        await LogEventAsync(consultEndMessage); 
    }

    
    private async Task LogEventAsync(string message)
    {
        using (StreamWriter writer = new StreamWriter("ward_log.txt", append: true)) 
        {
            await writer.WriteLineAsync($"{DateTime.Now}: {message}"); 
        }
    }
}

class InfectiousDiseaseWard
{
    private Semaphore _waitingRoomSemaphore; 
    private Queue<Patient> _patientQueue = new Queue<Patient>(); 
    private List<Doctor> _doctors = new List<Doctor>(); 
    private int _maxPatients; 
    private int _doctorCount; 
    private int _maxConsultationTime; 
    private object _queueLock = new object(); 

    
    public InfectiousDiseaseWard(int maxPatients, int doctorCount, int maxConsultationTime)
    {
        _maxPatients = maxPatients; 
        _doctorCount = doctorCount; 
        _maxConsultationTime = maxConsultationTime; 

        
        _waitingRoomSemaphore = new Semaphore(maxPatients, maxPatients);

        
        for (int i = 0; i < doctorCount; i++)
        {
            _doctors.Add(new Doctor());
        }
    }

    
    public async Task RunSimulation()
    {
        
        var doctorTasks = new List<Task>();
        foreach (var doctor in _doctors)
        {
            doctorTasks.Add(Task.Run(async () => await DoctorWork(doctor))); 
        }

        
        while (true)
        {
            
            Patient newPatient = GeneratePatient();
            string arrivalMessage = $"Пациент #{newPatient.Id} ({(newPatient.IsSick ? "больной" : "здоровый")}) пришел.";
            Console.WriteLine(arrivalMessage);
            await LogEventAsync(arrivalMessage); 

            
            if (_waitingRoomSemaphore.WaitOne(0)) 
            {
                lock (_queueLock)
                {
                    _patientQueue.Enqueue(newPatient); 
                }
                string enterMessage = $"Пациент #{newPatient.Id} заходит в приемное отделение.";
                Console.WriteLine(enterMessage);
                await LogEventAsync(enterMessage); 
            }
            else
            {
                
                string queueMessage = $"Приемное отделение заполнено. Пациент #{newPatient.Id} становится в очередь.";
                Console.WriteLine(queueMessage);
                await LogEventAsync(queueMessage); 
            }

            await Task.Delay(1000); 
        }
    }

    
    private async Task DoctorWork(Doctor doctor)
    {
        while (true) 
        {
            Patient patient = null;
            lock (_queueLock)
            {
                if (_patientQueue.Count > 0)
                {
                    patient = _patientQueue.Dequeue(); 
                }
            }

            if (patient != null) 
            {
                string consultMessage = $"Доктор #{doctor.Id} приступает к консультации пациента #{patient.Id} ({(patient.IsSick ? "больного" : "здорового")}).";
                Console.WriteLine(consultMessage);
                await LogEventAsync(consultMessage); 

                await doctor.ConsultPatient(patient, _maxConsultationTime); 

                _waitingRoomSemaphore.Release(); 
            }
            else
            {
                await Task.Delay(500); 
            }
        }
    }

    
    private Patient GeneratePatient()
    {
        bool isSick = new Random().Next(0, 2) == 0; 
        return new Patient(isSick); 
    }

    
    private async Task LogEventAsync(string message)
    {
        using (StreamWriter writer = new StreamWriter("ward_log.txt", append: true))
        {
            await writer.WriteLineAsync($"{DateTime.Now}: {message}"); 
        }
    }
}
