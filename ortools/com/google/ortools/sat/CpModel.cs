// Copyright 2010-2017 Google
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Google.OrTools.Sat
{
using System;
using System.Collections.Generic;

class IntervalVar
{

}

public class CpModel
{
  public CpModel()
  {
    model_ = new CpModelProto();
    constant_map_ = new Dictionary<long, int>();
  }

  // Getters.

  public CpModelProto Model
  {
    get { return model_; }
  }

  int Negated(int index)
  {
    return -index - 1;
  }

  // Integer variables and constraints.

  public IntVar NewIntVar(long lb, long ub, string name)
  {
    long[] bounds = { lb, ub };
    return new IntVar(model_, bounds, name);
  }

  public IntVar NewEnumeratedIntVar(IEnumerable<long> bounds, string name)
  {
    return new IntVar(model_, bounds, name);
  }

  // TODO: NewOptionalIntVar
  // TODO: NewOptionalEnumeratedIntVar

  // Constants (named or not).

  // TODO: Cache constant.
  public IntVar NewConstant(long value)
  {
    long[] bounds = { value, value };
    return new IntVar(model_, bounds, String.Format("{0}", value));
  }

  public IntVar NewConstant(long value, string name)
  {
    long[] bounds = { value, value };
    return new IntVar(model_, bounds, name);
  }

  // TODO: Add optional version of above 2 NewIntVar().

  public IntVar NewBoolVar(string name)
  {
    long[] bounds = { 0L, 1L };
    return new IntVar(model_, bounds, name);
  }

  public Constraint AddLinearConstraint(IEnumerable<Tuple<IntVar, long>> terms,
                                        long lb, long ub)
  {
    Constraint ct = new Constraint(model_);
    LinearConstraintProto lin = new LinearConstraintProto();
    foreach (Tuple<IntVar, long> term in terms)
    {
      lin.Vars.Add(term.Item1.Index);
      lin.Coeffs.Add(term.Item2);
    }
    lin.Domain.Add(lb);
    lin.Domain.Add(ub);
    ct.Proto.Linear = lin;
    return ct;
  }

  public Constraint AddLinearConstraint(IEnumerable<IntVar> vars,
                                        IEnumerable<long> coeffs,
                                        long lb, long ub)
  {
    Constraint ct = new Constraint(model_);
    LinearConstraintProto lin = new LinearConstraintProto();
    foreach (IntVar var in vars)
    {
      lin.Vars.Add(var.Index);
    }
    foreach (long coeff in coeffs)
    {
      lin.Coeffs.Add(coeff);
    }
    lin.Domain.Add(lb);
    lin.Domain.Add(ub);
    ct.Proto.Linear = lin;
    return ct;
  }

  public Constraint AddLinearConstraint(IEnumerable<IntVar> vars,
                                        IEnumerable<int> coeffs,
                                        long lb, long ub)
  {
    Constraint ct = new Constraint(model_);
    LinearConstraintProto lin = new LinearConstraintProto();
    foreach (IntVar var in vars)
    {
      lin.Vars.Add(var.Index);
    }
    foreach (int coeff in coeffs)
    {
      lin.Coeffs.Add(coeff);
    }
    lin.Domain.Add(lb);
    lin.Domain.Add(ub);
    ct.Proto.Linear = lin;
    return ct;
  }

  public Constraint AddSumConstraint(IEnumerable<IntVar> vars, long lb,
                                     long ub)
  {
    Constraint ct = new Constraint(model_);
    LinearConstraintProto lin = new LinearConstraintProto();
    foreach (IntVar var in vars)
    {
      lin.Vars.Add(var.Index);
      lin.Coeffs.Add(1L);
    }
    lin.Domain.Add(lb);
    lin.Domain.Add(ub);
    ct.Proto.Linear = lin;
    return ct;
  }

  // TODO: AddLinearConstraintWithBounds

  public Constraint Add(BoundIntegerExpression lin)
  {
    switch (lin.CtType)
    {
      case BoundIntegerExpression.Type.BoundExpression:
        {
          Dictionary<IntVar, long> dict = new Dictionary<IntVar, long>();
          long constant = IntegerExpression.GetVarValueMap(lin.Left, 1L, dict);
          Constraint ct = new Constraint(model_);
          LinearConstraintProto linear = new LinearConstraintProto();
          foreach (KeyValuePair<IntVar, long> term in dict)
          {
            linear.Vars.Add(term.Key.Index);
            linear.Coeffs.Add(term.Value);
          }
          linear.Domain.Add(lin.Lb == Int64.MinValue ? Int64.MinValue
                            : lin.Lb - constant);
          linear.Domain.Add(lin.Ub == Int64.MaxValue ? Int64.MaxValue
                            : lin.Ub - constant);
          ct.Proto.Linear = linear;
          return ct;
        }
      case BoundIntegerExpression.Type.VarEqVar:
        {
          Dictionary<IntVar, long> dict = new Dictionary<IntVar, long>();
          long constant = IntegerExpression.GetVarValueMap(lin.Left, 1L, dict);
          constant +=  IntegerExpression.GetVarValueMap(lin.Right, -1L, dict);
          Constraint ct = new Constraint(model_);
          LinearConstraintProto linear = new LinearConstraintProto();
          foreach (KeyValuePair<IntVar, long> term in dict)
          {
            linear.Vars.Add(term.Key.Index);
            linear.Coeffs.Add(term.Value);
          }
          linear.Domain.Add(-constant);
          linear.Domain.Add(-constant);
          ct.Proto.Linear = linear;
          return ct;
        }
      case BoundIntegerExpression.Type.VarDiffVar:
        {
          Dictionary<IntVar, long> dict = new Dictionary<IntVar, long>();
          long constant = IntegerExpression.GetVarValueMap(lin.Left, 1L, dict);
          constant +=  IntegerExpression.GetVarValueMap(lin.Right, -1L, dict);
          Constraint ct = new Constraint(model_);
          LinearConstraintProto linear = new LinearConstraintProto();
          foreach (KeyValuePair<IntVar, long> term in dict)
          {
            linear.Vars.Add(term.Key.Index);
            linear.Coeffs.Add(term.Value);
          }
          linear.Domain.Add(Int64.MinValue);
          linear.Domain.Add(-constant - 1);
          linear.Domain.Add(-constant + 1);
          linear.Domain.Add(Int64.MaxValue);
          ct.Proto.Linear = linear;
          return ct;
        }
      case BoundIntegerExpression.Type.VarEqCst:
        {
          Dictionary<IntVar, long> dict = new Dictionary<IntVar, long>();
          long constant = IntegerExpression.GetVarValueMap(lin.Left, 1L, dict);
          Constraint ct = new Constraint(model_);
          LinearConstraintProto linear = new LinearConstraintProto();
          foreach (KeyValuePair<IntVar, long> term in dict)
          {
            linear.Vars.Add(term.Key.Index);
            linear.Coeffs.Add(term.Value);
          }
          linear.Domain.Add(lin.Lb - constant);
          linear.Domain.Add(lin.Lb - constant);
          ct.Proto.Linear = linear;
          return ct;
        }
      case BoundIntegerExpression.Type.VarDiffCst:
        {
          Dictionary<IntVar, long> dict = new Dictionary<IntVar, long>();
          long constant = IntegerExpression.GetVarValueMap(lin.Left, 1L, dict);
          Constraint ct = new Constraint(model_);
          LinearConstraintProto linear = new LinearConstraintProto();
          foreach (KeyValuePair<IntVar, long> term in dict)
          {
            linear.Vars.Add(term.Key.Index);
            linear.Coeffs.Add(term.Value);
          }
          linear.Domain.Add(Int64.MinValue);
          linear.Domain.Add(lin.Lb - constant - 1);
          linear.Domain.Add(lin.Lb - constant + 1);
          linear.Domain.Add(Int64.MaxValue);
          ct.Proto.Linear = linear;
          return ct;
        }
    }
    return null;
  }

  public Constraint AddAllDifferent(IEnumerable<IntVar> vars)
  {
    Constraint ct = new Constraint(model_);
    AllDifferentConstraintProto alldiff = new AllDifferentConstraintProto();
    foreach (IntVar var in vars)
    {
      alldiff.Vars.Add(var.Index);
    }
    ct.Proto.AllDiff = alldiff;
    return ct;
  }

  public Constraint AddElement(IntVar index, IEnumerable<IntVar> vars,
                               IntVar target)
  {
    Constraint ct = new Constraint(model_);
    ElementConstraintProto element = new ElementConstraintProto();
    element.Index = index.Index;
    foreach (IntVar var in vars)
    {
      element.Vars.Add(var.Index);
    }
    element.Target = target.Index;
    ct.Proto.Element = element;
    return ct;
  }

  public Constraint AddElement(IntVar index, IEnumerable<long> values,
                               IntVar target)
  {
    Constraint ct = new Constraint(model_);
    ElementConstraintProto element = new ElementConstraintProto();
    element.Index = index.Index;
    foreach (long value in values)
    {
      element.Vars.Add(ConvertConstant(value));
    }
    element.Target = target.Index;
    ct.Proto.Element = element;
    return ct;
  }

  public Constraint AddElement(IntVar index, IEnumerable<int> values,
                               IntVar target)
  {
    Constraint ct = new Constraint(model_);
    ElementConstraintProto element = new ElementConstraintProto();
    element.Index = index.Index;
    foreach (int value in values)
    {
      element.Vars.Add(ConvertConstant(value));
    }
    element.Target = target.Index;
    ct.Proto.Element = element;
    return ct;
  }

  public Constraint AddCircuit(IEnumerable<Tuple<int, int, ILiteral>> arcs)
  {
    Constraint ct = new Constraint(model_);
    CircuitConstraintProto circuit = new CircuitConstraintProto();
    foreach (var arc in arcs)
    {
      circuit.Tails.Add(arc.Item1);
      circuit.Heads.Add(arc.Item2);
      circuit.Literals.Add(arc.Item3.GetIndex());
    }
    ct.Proto.Circuit = circuit;
    return ct;
  }

  public Constraint AddAllowedAssignments(IEnumerable<IntVar> vars,
                                          long[,] tuples)
  {
    Constraint ct = new Constraint(model_);
    TableConstraintProto table = new TableConstraintProto();
    foreach (IntVar var in vars)
    {
      table.Vars.Add(var.Index);
    }
    for (int i = 0; i < tuples.GetLength(0); ++i)
    {
      for (int j = 0; j < tuples.GetLength(1);++j)
      {
        table.Values.Add(tuples[i, j]);
      }
    }
    ct.Proto.Table = table;
    return ct;
  }

  public Constraint AddForbiddenAssignments(IEnumerable<IntVar> vars,
                                            long[,] tuples)
  {
    Constraint ct = AddAllowedAssignments(vars, tuples);
    ct.Proto.Table.Negated = true;
    return ct;
  }

  // TODO: AddAutomata

  public Constraint AddInverse(IEnumerable<IntVar> direct,
                               IEnumerable<IntVar> reverse)
  {
    Constraint ct = new Constraint(model_);
    InverseConstraintProto inverse = new InverseConstraintProto();
    foreach (IntVar var in direct)
    {
      inverse.FDirect.Add(var.Index);
    }
    foreach (IntVar var in reverse)
    {
      inverse.FInverse.Add(var.Index);
    }
    ct.Proto.Inverse = inverse;
    return ct;
  }

  // TODO: AddReservoirConstraint

  // TODO: AddMapDomain

  public Constraint AddImplication(ILiteral a, ILiteral b)
  {
    Constraint ct = new Constraint(model_);
    BoolArgumentProto or = new BoolArgumentProto();
    or.Literals.Add(a.Not().GetIndex());
    or.Literals.Add(b.GetIndex());
    ct.Proto.BoolOr = or;
    return ct;
  }

  public Constraint AddBoolOr(IEnumerable<ILiteral> literals)
  {
    Constraint ct = new Constraint(model_);
    BoolArgumentProto bool_argument = new BoolArgumentProto();
    foreach (ILiteral lit in literals)
    {
      bool_argument.Literals.Add(lit.GetIndex());
    }
    ct.Proto.BoolOr = bool_argument;
    return ct;
  }

  public Constraint AddBoolAnd(IEnumerable<ILiteral> literals)
  {
    Constraint ct = new Constraint(model_);
    BoolArgumentProto bool_argument = new BoolArgumentProto();
    foreach (ILiteral lit in literals)
    {
      bool_argument.Literals.Add(lit.GetIndex());
    }
    ct.Proto.BoolAnd = bool_argument;
    return ct;
  }

  public Constraint AddBoolXor(IEnumerable<ILiteral> literals)
  {
    Constraint ct = new Constraint(model_);
    BoolArgumentProto bool_argument = new BoolArgumentProto();
    foreach (ILiteral lit in literals)
    {
      bool_argument.Literals.Add(lit.GetIndex());
    }
    ct.Proto.BoolXor = bool_argument;
    return ct;
  }

  public Constraint AddMinEquality(IntVar target, IEnumerable<IntVar> vars)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    foreach (IntVar var in vars)
    {
      args.Vars.Add(var.Index);
    }
    args.Target = target.Index;
    ct.Proto.IntMin = args;
    return ct;
  }

  public Constraint AddMaxEquality(IntVar target, IEnumerable<IntVar> vars)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    foreach (IntVar var in vars)
    {
      args.Vars.Add(var.Index);
    }
    args.Target = target.Index;
    ct.Proto.IntMax = args;
    return ct;
  }

  public Constraint AddDivisionEquality(IntVar x, IntVar y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(x.Index);
    args.Vars.Add(y.Index);
    ct.Proto.IntDiv = args;
    return ct;
  }

  public Constraint AddDivisionEquality(IntVar x, long y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(x.Index);
    args.Vars.Add(ConvertConstant(y));
    ct.Proto.IntDiv = args;
    return ct;
  }

  public Constraint AddDivisionEquality(long x, IntVar y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(ConvertConstant(x));
    args.Vars.Add(y.Index);
    ct.Proto.IntDiv = args;
    return ct;
  }

  public Constraint AddModuloEquality(IntVar x, IntVar y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(x.Index);
    args.Vars.Add(y.Index);
    ct.Proto.IntMod = args;
    return ct;
  }

  public Constraint AddModuloEquality(IntVar x, long y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(x.Index);
    args.Vars.Add(ConvertConstant(y));
    ct.Proto.IntMod = args;
    return ct;
  }

  public Constraint AddModuloEquality(long x, IntVar y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(ConvertConstant(x));
    args.Vars.Add(y.Index);
    ct.Proto.IntMod = args;
    return ct;
  }

  public Constraint AddProdEquality(IntVar x, IntVar y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(x.Index);
    args.Vars.Add(y.Index);
    ct.Proto.IntProd = args;
    return ct;
  }

  public Constraint AddProdEquality(IntVar x, long y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(x.Index);
    args.Vars.Add(ConvertConstant(y));
    ct.Proto.IntProd = args;
    return ct;
  }

  public Constraint AddProdEquality(long x, IntVar y)
  {
    Constraint ct = new Constraint(model_);
    IntegerArgumentProto args = new IntegerArgumentProto();
    args.Vars.Add(ConvertConstant(x));
    args.Vars.Add(y.Index);
    ct.Proto.IntProd = args;
    return ct;
  }

  // Scheduling support

  // TODO: NewInterval

  // Objective.
  public void Minimize(IntegerExpression obj)
  {
    SetObjective(obj, true);
  }

  public void Maximize(IntegerExpression obj)
  {
    SetObjective(obj, false);
  }

  bool HasObjective()
  {
    return model_.Objective == null;
  }

  // Internal methods.

  void SetObjective(IntegerExpression obj, bool minimize)
  {
    CpObjectiveProto objective = new CpObjectiveProto();
    if (obj is IntVar)
    {
      objective.Coeffs.Add(1L);
      objective.Offset = 0L;
      if (minimize)
      {
        objective.Vars.Add(obj.Index);
        objective.ScalingFactor = 1L;
      }
      else
      {
        objective.Vars.Add(Negated(obj.Index));
        objective.ScalingFactor = -1L;
      }
    }
    else
    {
      Dictionary<IntVar, long> dict = new Dictionary<IntVar, long>();
      long constant = IntegerExpression.GetVarValueMap(obj, 1L, dict);
      if (minimize)
      {
        objective.ScalingFactor = 1L;
        objective.Offset = constant;
      }
      else
      {
        objective.ScalingFactor = -1L;
        objective.Offset = -constant;
      }
      foreach (KeyValuePair<IntVar, long> it in dict)
      {
        objective.Coeffs.Add(it.Value);
        if (minimize)
        {
          objective.Vars.Add(it.Key.Index);
        }
        else
        {
          objective.Vars.Add(Negated(it.Key.Index));
        }
      }
    }
    model_.Objective = objective;
  }

  private int ConvertConstant(long value)
  {
    if (constant_map_.ContainsKey(value))
    {
      return constant_map_[value];
    }
    else
    {
      int index = model_.Variables.Count;
      IntegerVariableProto var = new IntegerVariableProto();
      var.Domain.Add(value);
      var.Domain.Add(value);
      constant_map_.Add(value, index);
      return index;
    }
  }

  private CpModelProto model_;
  private Dictionary<long, int> constant_map_;
}

}  // namespace Google.OrTools.Sat