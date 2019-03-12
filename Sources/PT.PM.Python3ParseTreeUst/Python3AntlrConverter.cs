using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Python3ParseTreeUst
{
    public partial class Python3AntlrConverter : AntlrConverter, IPython3ParserVisitor<Ust>
    {
        public override Language Language => Language.Python3;


        public Ust VisitRoot(Python3Parser.RootContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSingle_input(Python3Parser.Single_inputContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFile_input(Python3Parser.File_inputContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEval_input(Python3Parser.Eval_inputContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecorator(Python3Parser.DecoratorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecorators(Python3Parser.DecoratorsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDecorated(Python3Parser.DecoratedContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAsync_funcdef(Python3Parser.Async_funcdefContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFuncdef(Python3Parser.FuncdefContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitParameters(Python3Parser.ParametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypedargslist(Python3Parser.TypedargslistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTfpdef(Python3Parser.TfpdefContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVarargslist(Python3Parser.VarargslistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVfpdef(Python3Parser.VfpdefContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStmt(Python3Parser.StmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSimple_stmt(Python3Parser.Simple_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSmall_stmt(Python3Parser.Small_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpr_stmt(Python3Parser.Expr_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnassign(Python3Parser.AnnassignContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTestlist_star_expr(Python3Parser.Testlist_star_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAugassign(Python3Parser.AugassignContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDel_stmt(Python3Parser.Del_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPass_stmt(Python3Parser.Pass_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFlow_stmt(Python3Parser.Flow_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitBreak_stmt(Python3Parser.Break_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitContinue_stmt(Python3Parser.Continue_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitReturn_stmt(Python3Parser.Return_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitYield_stmt(Python3Parser.Yield_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitRaise_stmt(Python3Parser.Raise_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_stmt(Python3Parser.Import_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_name(Python3Parser.Import_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_from(Python3Parser.Import_fromContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_as_name(Python3Parser.Import_as_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_as_name(Python3Parser.Dotted_as_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImport_as_names(Python3Parser.Import_as_namesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_as_names(Python3Parser.Dotted_as_namesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDotted_name(Python3Parser.Dotted_nameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGlobal_stmt(Python3Parser.Global_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNonlocal_stmt(Python3Parser.Nonlocal_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAssert_stmt(Python3Parser.Assert_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitCompound_stmt(Python3Parser.Compound_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAsync_stmt(Python3Parser.Async_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitIf_stmt(Python3Parser.If_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWhile_stmt(Python3Parser.While_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFor_stmt(Python3Parser.For_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTry_stmt(Python3Parser.Try_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWith_stmt(Python3Parser.With_stmtContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitWith_item(Python3Parser.With_itemContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExcept_clause(Python3Parser.Except_clauseContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSuite(Python3Parser.SuiteContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTest(Python3Parser.TestContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTest_nocond(Python3Parser.Test_nocondContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLambdef(Python3Parser.LambdefContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLambdef_nocond(Python3Parser.Lambdef_nocondContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitOr_test(Python3Parser.Or_testContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnd_test(Python3Parser.And_testContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNot_test(Python3Parser.Not_testContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComparison(Python3Parser.ComparisonContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_op(Python3Parser.Comp_opContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitStar_expr(Python3Parser.Star_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExpr(Python3Parser.ExprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitXor_expr(Python3Parser.Xor_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnd_expr(Python3Parser.And_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitShift_expr(Python3Parser.Shift_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArith_expr(Python3Parser.Arith_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTerm(Python3Parser.TermContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFactor(Python3Parser.FactorContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPower(Python3Parser.PowerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAtom_expr(Python3Parser.Atom_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAtom(Python3Parser.AtomContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTestlist_comp(Python3Parser.Testlist_compContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTrailer(Python3Parser.TrailerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubscriptlist(Python3Parser.SubscriptlistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSubscript(Python3Parser.SubscriptContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSliceop(Python3Parser.SliceopContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExprlist(Python3Parser.ExprlistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTestlist(Python3Parser.TestlistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDictorsetmaker(Python3Parser.DictorsetmakerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassdef(Python3Parser.ClassdefContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArglist(Python3Parser.ArglistContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArgument(Python3Parser.ArgumentContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_iter(Python3Parser.Comp_iterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_for(Python3Parser.Comp_forContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitComp_if(Python3Parser.Comp_ifContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEncoding_decl(Python3Parser.Encoding_declContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitYield_expr(Python3Parser.Yield_exprContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitYield_arg(Python3Parser.Yield_argContext context)
        {
            return VisitChildren(context);
        }
    }
}