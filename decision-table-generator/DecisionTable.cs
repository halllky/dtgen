using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DTGen
{
    public class DecisionTable
    {
        public string[][] GetTable(Direction direction = Direction.Vertical)
        {
            foreach (var cons in constraints) cons.ThrowIfInvalid();

            var combinations = conditions
                .Aggregate(
                    Enumerable.Empty<IEnumerable<KeyValuePair<string, string>>>(),
                    (prev, curr) => prev.Any()
                        ? prev.SelectMany(p => curr.Value.Select(value => KeyValuePair.Create(curr.Key, value)), (p, c) => p.Concat(new[] { c }))
                        : curr.Value.Select(value => new[] { KeyValuePair.Create(curr.Key, value) }))
                .Select(column => column.ToDictionary(item => item.Key, item => item.Value))
                .Where(column => constraints.All(cons => cons.IsFilteredIn(column)))
                .ToArray();

            switch (direction)
            {
                case Direction.Vertical:
                    var header = new[] { conditions.Select(cond => cond.Key).Concat(actions).ToArray() };
                    var body = combinations.Select(combi => conditions
                        .Select(cond => combi[cond.Key])
                        .Concat(Enumerable.Repeat("", actions.Count))
                        .ToArray());
                    return header.Concat(body).ToArray();

                case Direction.Horizontal:
                    var rows = conditions
                        .Select(cond => new[] { cond.Key }.Concat(combinations.Select(col => col[cond.Key])).ToArray())
                        .ToList();
                    rows.AddRange(actions
                        .Select(action => new[] { action }.Concat(Enumerable.Repeat("", combinations.Length))
                        .ToArray()));
                    return rows.ToArray();

                default:
                    throw new NotImplementedException();
            }
        }
        public string ToString(string separator, Direction direction = Direction.Vertical)
        {
            return string.Join(Environment.NewLine, GetTable(direction).Select(row => string.Join(separator ?? "\t", row)));
        }
        public override string ToString()
        {
            return ToString("\t");
        }


        public void AddCondition(string arg)
        {
            var regex = new Regex("^([^:]+):?(.*)$");
            var match = regex.Match(arg);
            if (!match.Success || match.Groups.Count < 2)
                throw new UsageException($"Invalid condition: \"{arg}\". " + CONDITION_USAGE);

            var key = match.Groups[1].Value.Trim();
            var values = match.Groups.Count >= 3 && !string.IsNullOrWhiteSpace(match.Groups[2].Value)
                ? match.Groups[2].Value.Split(",").Select(v => v.Trim()).ToArray()
                : new[] { "T", "F" };
            if (values.Any()) conditions[key] = values;
        }
        public void ClearCondition() => conditions.Clear();
        private readonly Dictionary<string, string[]> conditions = new Dictionary<string, string[]>();


        public void AddAction(string arg) => actions.Add(arg);
        public void ClearAction() => actions.Clear();
        private readonly List<string> actions = new List<string>();


        public void AddConstraint(string arg)
        {
            var regex = new Regex(@"^\s*if\s+(.+)\s+(==|!=|in|!in)\s+(.+)\s+then\s+(.+)\s+(=|!=|in|!in)\s+(.+)$", RegexOptions.IgnoreCase);
            var match = regex.Match(arg);
            if (!match.Success || match.Groups.Count != 7)
                throw new UsageException($"Invalid constraint: \"{arg}\". " + CONSTRAINT_USAGE);

            constraints.Add(new Constraint
            {
                Source = this,
                FilterConditionName = match.Groups[1].Value.Trim(),
                FilterOperator = match.Groups[2].Value.Trim().ToLower(),
                FilterValues = match.Groups[3].Value.Split(",").Select(v => v.Trim()).ToArray(),
                RestrictedConditionName = match.Groups[4].Value.Trim(),
                RestrictedOperator = match.Groups[5].Value.Trim().ToLower(),
                RestrictedValues = match.Groups[6].Value.Split(",").Select(v => v.Trim()).ToArray(),
            });
        }
        public void ClearConstraint() => constraints.Clear();
        private readonly List<Constraint> constraints = new List<Constraint>();
        private class Constraint
        {
            internal DecisionTable Source { get; set; }
            internal string FilterConditionName { get; set; }
            internal string FilterOperator { get; set; }
            internal string[] FilterValues { get; set; }
            internal string RestrictedConditionName { get; set; }
            internal string RestrictedOperator { get; set; }
            internal string[] RestrictedValues { get; set; }

            internal bool IsFilteredIn(Dictionary<string, string> conditions)
            {
                if (!conditions.TryGetValue(FilterConditionName, out var conditionValue))
                    throw new UsageException($"Condition \"{FilterConditionName}\" doesn't exist.");
                if (!conditions.TryGetValue(RestrictedConditionName, out var restriceTargetValue))
                    throw new UsageException($"Condition \"{RestrictedConditionName}\" doesn't exist.");

                switch (FilterOperator)
                {
                    case "==": if (conditionValue != FilterValues.Single()) return true; break;
                    case "!=": if (conditionValue == FilterValues.Single()) return true; break;
                    case "in": if (!FilterValues.Contains(conditionValue)) return true; break;
                    case "!in": if (FilterValues.Contains(conditionValue)) return true; break;
                }
                switch (RestrictedOperator)
                {
                    case "=": if (restriceTargetValue == RestrictedValues.Single()) return true; break;
                    case "!=": if (restriceTargetValue != RestrictedValues.Single()) return true; break;
                    case "in": if (RestrictedValues.Contains(restriceTargetValue)) return true; break;
                    case "!in": if (!RestrictedValues.Contains(restriceTargetValue)) return true; break;
                }
                return false;
            }

            internal void ThrowIfInvalid()
            {
                if (!Source.conditions.TryGetValue(FilterConditionName, out var conditionValue))
                    throw new UsageException($"Condition \"{FilterConditionName}\" doesn't exist.");

                var invalidFilterValue = FilterValues.Except(conditionValue).FirstOrDefault();
                if (invalidFilterValue != null)
                    throw new UsageException($"Condition \"{FilterConditionName}\" will never be set to \"{invalidFilterValue}\".");

                if (!Source.conditions.TryGetValue(RestrictedConditionName, out var restriceTargetValue))
                    throw new UsageException($"Condition \"{RestrictedConditionName}\" doesn't exist.");

                var invalidRestrictedValue = RestrictedValues.Except(restriceTargetValue).FirstOrDefault();
                if (invalidRestrictedValue != null)
                    throw new UsageException($"Condition \"{RestrictedConditionName}\" will never be set to \"{invalidRestrictedValue}\".");
            }
        }

        public enum Direction { Vertical, Horizontal }
        internal const string CONDITION_USAGE = "A condition must be written in format like \"condition1:value1,value2\".";
        internal const string CONSTRAINT_USAGE = "A constraint must be written in format like \"IF condition1 == value1 THEN condition2 !in value2,value3,value4\". (\"if\" operator: ==, !=, in, !in) (\"then\" operator: =, !=, in, !in)";
    }
}
