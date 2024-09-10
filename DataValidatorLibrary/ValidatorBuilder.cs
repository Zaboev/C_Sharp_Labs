using System;  
using System.Collections.Generic;  
using System.Linq;  

namespace DataValidatorLibrary 
{
    
    public interface IValidationRule<T>
    {
        
        void Validate(T data);
    }

    public class ValidatorBuilder<T>
    {
        
        private readonly List<IValidationRule<T>> _rules = new List<IValidationRule<T>>();

        
        public ValidatorBuilder<T> AddRule(IValidationRule<T> rule)
        {
            _rules.Add(rule);  
            return this;  
        }

       
        public DataValidator<T> Build()
        {
            if (!_rules.Any())
            {
                throw new InvalidOperationException("No validation rules added.");
            }

            return new DataValidator<T>(_rules);
        }
    }

    public class DataValidator<T>
    {
        private readonly IEnumerable<IValidationRule<T>> _rules;

     
        public DataValidator(IEnumerable<IValidationRule<T>> rules)
        {
            _rules = rules;  
        }

        public void Validate(T data)
        {
            var exceptions = new List<Exception>();

            foreach (var rule in _rules)
            {
                try
                {
                    rule.Validate(data);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message){ }
    }
}
