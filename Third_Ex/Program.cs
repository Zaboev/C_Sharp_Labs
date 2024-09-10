using System; 
using System.Collections.Generic; 
using DataValidatorLibrary; 

namespace DataValidatorConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var stringValidator = new ValidatorBuilder<string>() 
                .AddRule(new NotNullRule<string>()) 
                .AddRule(new StringLengthRule(5, 10)) 
                .Build(); 

            
            var stringTestData = new List<string> { "hello", null, "world", "C#", "12345678901" };

            
            foreach (var data in stringTestData)
            {
                try
                {
                    stringValidator.Validate(data); 
                    Console.WriteLine($"'{data}' is valid."); 
                }
                catch (AggregateException ex) 
                {
                    
                    Console.WriteLine($"Validation failed for '{data}': {string.Join("; ", ex.InnerExceptions.Select(e => e.Message))}");
                }
            }

            
            var intValidator = new ValidatorBuilder<int>() 
                .AddRule(new PositiveNumberRule()) 
                .Build(); 

           
            var intTestData = new List<int> { 10, -5, 0, 20 };

            
            foreach (var number in intTestData)
            {
                try
                {
                    intValidator.Validate(number); 
                    Console.WriteLine($"'{number}' is valid."); 
                }
                catch (AggregateException ex) 
                {
                    
                    Console.WriteLine($"Validation failed for '{number}': {string.Join("; ", ex.InnerExceptions.Select(e => e.Message))}");
                }
            }
        }
    }
}