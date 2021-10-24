using System;
using Microsoft.Extensions.CommandLineUtils;

namespace DTGen
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: true);

            app.Name = "decision table generator";
            app.HelpOption("-h|--help");

            var conditions = app.Argument(
                name: "condition",
                description: DecisionTable.CONDITION_USAGE,
                multipleValues: true);
            var constraints = app.Option(
                template: "-c|--constraint",
                description: DecisionTable.CONSTRAINT_USAGE+ " (multiple)",
                optionType: CommandOptionType.MultipleValue);
            var actions = app.Option(
                template: "-a|--action",
                description: "Action to take when the specified condition is met. (multiple)",
                optionType: CommandOptionType.MultipleValue);
            var separator = app.Option(
                template: "-s|--separator",
                description: "Output separator.",
                optionType: CommandOptionType.SingleValue);
            var horizontal = app.Option(
                template: "-H|--horizontal",
                description: "Table will extend horizontally.",
                optionType: CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                if (conditions.Values.Count == 0)
                {
                    app.ShowHelp();
                    return 1;
                }

                var dt = new DecisionTable();
                foreach (var cond in conditions.Values) dt.AddCondition(cond);
                foreach (var constraint in constraints.Values) dt.AddConstraint(constraint);
                foreach (var expect in actions.Values) dt.AddAction(expect);

                try
                {
                    var direction = horizontal.HasValue() ? DecisionTable.Direction.Horizontal : DecisionTable.Direction.Vertical;
                    Console.WriteLine(dt.ToString(separator.Value(), direction));
                    return 0;
                }
                catch (UsageException ex)
                {
                    Console.Error.WriteLine("ERROR: " + ex.Message);
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    return 1;
                }
            });

            return app.Execute(args);
        }
    }
}
