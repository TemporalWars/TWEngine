#region File Description
//-----------------------------------------------------------------------------
// KeyboardHelper.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Console.Structs;
using Microsoft.Xna.Framework.Input;

namespace ImageNexus.BenScharbach.TWEngine.Console
{
    /// <summary>
    /// The <see cref="KeyboardHelper"/> populates the <see cref="KeyBinding"/> collection.
    /// </summary>
    class KeyboardHelper
    {
        static public KeyBinding[]	ItalianBindings = new[]
                                  	                      {
                                  	                          new KeyBinding( Keys.OemPipe,			'\\',	'|',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.OemBackslash,		'<',	'>',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.OemOpenBrackets,	'\'',	'?',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.OemCloseBrackets,	'ì',	'^',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.OemSemicolon,		'è',	'é',		'[',		'{' ),
                                  	                          new KeyBinding( Keys.OemPlus,			'+',	'*',		']',		'}' ),
                                  	                          new KeyBinding( Keys.OemTilde,			'ò',	'ç',		'@',		(char)0 ),
                                  	                          new KeyBinding( Keys.OemQuotes,			'à',	'°',		'#',		(char)0 ),
                                  	                          new KeyBinding( Keys.OemQuestion,		'ù',	'§',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.OemComma,			',',	';',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.OemPeriod,			'.',	':',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.OemMinus,			'-',	'_',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.Space,				' ',	(char)0,	(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D1,				'1',	'!',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D2,				'2',	'\"',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D3,				'3',	'£',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D4,				'4',	'$',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D5,				'5',	'%',		'€',		(char)0 ),
                                  	                          new KeyBinding( Keys.D6,				'6',	'&',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D7,				'7',	'/',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D8,				'8',	'(',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D9,				'9',	')',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D0,				'0',	'=',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.A,					'a',	'A',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.B,					'b',	'B',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.C,					'c',	'C',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.D,					'd',	'D',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.E,					'e',	'E',		'€',		(char)0 ),
                                  	                          new KeyBinding( Keys.F,					'f',	'F',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.G,					'g',	'G',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.H,					'h',	'H',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.I,					'i',	'I',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.J,					'j',	'J',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.K,					'k',	'K',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.L,					'l',	'L',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.M,					'm',	'M',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.N,					'n',	'N',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.O,					'o',	'O',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.P,					'p',	'P',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.Q,					'q',	'Q',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.R,					'r',	'R',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.S,					's',	'S',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.T,					't',	'T',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.U,					'u',	'U',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.V,					'v',	'V',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.W,					'w',	'W',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.X,					'x',	'X',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.Y,					'y',	'Y',		(char)0,	(char)0 ),
                                  	                          new KeyBinding( Keys.Z,					'z',	'Z',		(char)0,	(char)0 )
                                  	                      };

        static public KeyBinding[]	AmericanBindings = new[]
                                  	                       {
                                  	                           new KeyBinding( Keys.OemTilde,			'`',	'~',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemBackslash,		'\\',	'|',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemMinus,      	'-',	'_',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemPlus,	        '=',	'+',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemOpenBrackets,	'[',	'{',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemCloseBrackets,	']',	'}',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemSemicolon,		';',	':',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemQuotes,			'\'',	'"',		(char)0,	(char)0 ),
                                  	                           //new KeyBinding( Keys.OemQuestion,		'\\',	'|',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemComma,			',',	'<',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemPeriod,			'.',	'>',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.OemQuestion,		'/',	'?',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.Space,				' ',	(char)0,	(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D1,				'1',	'!',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D2,				'2',	'@',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D3,				'3',	'#',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D4,				'4',	'$',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D5,				'5',	'%',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D6,				'6',	'^',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D7,				'7',	'&',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D8,				'8',	'*',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D9,				'9',	'(',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D0,				'0',	')',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.A,					'a',	'A',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.B,					'b',	'B',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.C,					'c',	'C',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.D,					'd',	'D',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.E,					'e',	'E',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.F,					'f',	'F',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.G,					'g',	'G',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.H,					'h',	'H',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.I,					'i',	'I',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.J,					'j',	'J',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.K,					'k',	'K',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.L,					'l',	'L',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.M,					'm',	'M',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.N,					'n',	'N',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.O,					'o',	'O',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.P,					'p',	'P',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.Q,					'q',	'Q',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.R,					'r',	'R',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.S,					's',	'S',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.T,					't',	'T',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.U,					'u',	'U',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.V,					'v',	'V',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.W,					'w',	'W',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.X,					'x',	'X',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.Y,					'y',	'Y',		(char)0,	(char)0 ),
                                  	                           new KeyBinding( Keys.Z,					'z',	'Z',		(char)0,	(char)0 )
                                  	                       };
    }
}