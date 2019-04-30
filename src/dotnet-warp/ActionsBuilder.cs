using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotnetWarp.CmdCommands;
using DotnetWarp.CmdCommands.Options;

namespace DotnetWarp
{
    internal class ActionsBuilder
    {
        public IEnumerable<Expression<Func<Context, bool>>> GetActionsForContext(Context context)
        {
            var actions = new List<Expression<Func<Context, bool>>>();

            var dotnetCli = new DotnetCli(context.ProjectFileOrFolder, context.IsVerbose, context.MsBuildProperties);
            var warp = new WarpCli(context.CurrentPlatform, context.IsVerbose);

            var wasLinkerPackageAdded = false;
            if (context.ShouldAddLinkerPackage)
            {
                wasLinkerPackageAdded = true;
                actions.Add(ctx => dotnetCli.AddLinkerPackage());
            }

            actions.Add(ctx =>
                dotnetCli.Publish(ctx, new DotnetPublishOptions(context.Rid, context.ShouldNotRootApplicationAssemblies, context.IsNoCrossGen)));

            actions.Add(ctx => warp.Pack(ctx, new WarpPackOptions(context.OutputPath)));

            if (wasLinkerPackageAdded)
            {
                actions.Add(ctx => dotnetCli.RemoveLinkerPackage());
            }

            return actions;
        }
    }
}