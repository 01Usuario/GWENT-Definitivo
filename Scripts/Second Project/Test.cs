using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestScript:MonoBehaviour
{
    void Start()
    {
        List<string> testCases = new List<string> { testCase0 };

        foreach (var testCase in testCases)
        {
            try
            {
                // Lexer
                Lexer lexer = new Lexer(testCase);
                List<Token> tokens = lexer.Tokenize();
                Debug.Log("Lexer passed.");

                // Parser
                Parser parser = new Parser(tokens);
                NodeAST ast = parser.Parse();
                Debug.Log("Parser passed.");

                // AST Validator
                ASTValidator astValidator = new ASTValidator();
                astValidator.Validate(ast);
                Debug.Log("AST validation passed.");

                // Semantic Validator
                SemanticValidator semanticValidator = new SemanticValidator();
                Debug.Log("Comenzo el semantico");
                ast.Accept(semanticValidator);
                Debug.Log("Semantic validation passed.");

                Debug.Log("Test case passed.\n");
            }
            catch (Exception ex)
            {
                throw new Exception($"Test case failed: {ex.Message}\n");
            }
        }
    }

    // Casos de prueba
    static string testCase0 = @"
    
   effect {
        Name: ""Damage"",
        Params: {
            Amount: Number
        },
        Action: (targets, context) => {
            for target in targets {
                i = 0;
                while (i++ < Amount) {
                    target.Power -= 1;
                }
            }
        }
    }
    effect {
        Name: ""Draw"",
        Action: (targets, context) => {
            topCard = context.Deck.Pop();
            context.Hand.Add(topCard);
            context.Hand.Shuffle();
        }
    }
    effect {
        Name: ""ReturnToDeck"",
        Action: (targets, context) => {
            for target in targets {
                owner = target.Owner;
                deck = context.DeckOfPlayer(owner);
                deck.Push(target);
                deck.Shuffle();
                context.Board.Remove(target);
            }
        }
    }
    card {
        Type: ""Oro"",
        Name: ""Beluga"",
        Faction: ""Northern Realms"",
        Power: 10,
        Range: [""Melee"", ""Ranged""],
        OnActivation: [
            {
                effect: {
                    Name: ""Damage"",
                    Amount: 5,
                    Selector: {
                        Source: ""board"",
                        Single: false,
                        Predicate: (unit) => unit.Faction == ""Northern"" @@ ""Realms""
                    },
                    PostAction: {
                        Type: ""ReturnToDeck"",
                        Selector: {
                            Source: ""parent"",
                            Single: false,
                            Predicate: (unit) => unit.Power < 1
                        },
                    }
                }
            },
            {
                effect: ""Draw""
            }
        ]
    }";
    static string testCase3 = @"
    effect {
        Name: ""Heal"",
        Params: {
            Amount: Number
        },
        Action: (targets, context) => {
            for target in targets {
                target.Power -= Amount;
            }
        }
    }
      
    card {
        Type: ""Plata"",
        Name: ""Griffin"",
        Faction: ""Monsters"",
        Power: 8,
        Range: [""Melee""],
        OnActivation: [
            {
                effect: {
                    Name: ""Heal"",
                    Amount: 3,
                    Selector: {
                        Source: ""board"",
                        Single: true,
                        Predicate: (unit) => unit.Faction == ""Monsters""
                    }
                }
            }
        ]
    }
    ";

    static string testCase7 = @"
    effect {
        Name: ""Boost"",
        Params: {
            Amount: Number
        },
        Action: (targets, context) => {
            for target in targets {
                target.Power += Amount;
            }
        }
    }

    card {
        Type: ""Oro"",
        Name: ""Dandelion"",
        Faction: ""Northern Realms"",
        Power: 4,
        Range: [""Ranged""],
        OnActivation: [
            {
                effect: {
                    Name: ""Boost"",
                    Amount: 2,
                    Selector: {
                        Source: ""hand"",
                        Single: false,
                        Predicate: (unit) => unit.Faction == ""Northern"" @@ ""Realms""
                    }
                }
            }
        ]
    }
    ";
}
