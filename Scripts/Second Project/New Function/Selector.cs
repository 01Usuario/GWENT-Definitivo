using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Selector
{
    public string Source { get; set; }
    public bool Single { get; set; }
    public Predicate<CardSO> Predicate { get; set; }
}

