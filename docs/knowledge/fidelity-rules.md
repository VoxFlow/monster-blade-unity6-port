# Fidelity rules

The rules that governed changes in this port. They are written to be reusable for other preservation ports.

1. **The original's code, data and engine behaviour come first.** A change is justified by what the original does, established from
   its code, its data or a measurement of its engine, not by what seems reasonable.
2. **No value tuning.** A difference is not fixed by adjusting a number until the output matches. Its cause is found and the
   faithful mechanism is reproduced.
3. **Prefer data over code, and build time over run time.** If a difference can be removed by restoring the original data, that is
   preferred to a run-time workaround.
4. **Every difference is registered.** Each entry states the class of difference, the evidence for the original behaviour, why data
   alone cannot reproduce it, the cost, the alternatives tested, the acceptance test and how to reverse it.
5. **Label evidence.** Each claim is proven (measured or read directly), supported (consistent evidence, not measured) or inferred.
   Overclaims are corrected when noticed.
6. **Substitutes are labelled.** Where the original is lost (server rules), substitute rules are derived from the original's own data
   and displays and marked as substitutes in code and documentation.
7. **Original defects are kept.** Behaviour that the original exhibits is reproduced, including its defects. Correcting one would
   be a separate, explicit decision.
8. **Hardware differences are named as such.** When a difference comes from much faster hardware rather than from the engine, the
   fix is labelled a quality fix and kept minimal.
9. **Disproved hypotheses are recorded.** A cause that was tested and ruled out is written down with its evidence, so it is not
   investigated again.
