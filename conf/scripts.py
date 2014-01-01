import math

class SnipClass():
	# How create function:
	#   1 - Define a name
	#   2 - Determine is the second parameter is a value or an element
	#   3 - Add your parameters
	#   4 - Define the prototype in the PROTOTYPE between section #proto: START and #proto: END (see sample)
	#   5 - Create the function in the FUNCTIONS section always like this
	#                  Myfunc1(self, element, value, param)
	#
	#   param is an array that contains parameters defined in prototype section
	#
    # Be carreful: The return type must be the same that the defined type in RecordStructure.xml in type attibut (type="float")
    #  	
	# Sample with value and 2 parameters :
	# Prototype : 
	#        #ReturnZeroIfEqualRounded(self, element, value, float, int)
	#
	# Function : param[0] is float parameter in prototype     param[1] is int parameter in prototype
	#        	def ReturnZeroIfEqualRounded(self, element, value, param):
	#        		testValue = param[0]
	#        		rounded = param[1]
	#        		value = round(value, rounded)
	#        		if testValue == value:
	#        			return 0.0
	#        		else:
	#        			return value
	#
	# Sample use in RecordStructure.xml : 
	#      funcr: is used function when snip read the parameter
	#      funcw: is used function when snip write the parameter if the parameter if changer in the edit form of snip
	#
	#      <Element name="Directional *" type="float" desc="(this # x57.29578)" funcr="ReturnZeroIfEqual(3.40282347E+38)" funcw="ReturnValueIfEqualZero(3.40282347E+38)"/> 
	#
	#      <Element name="Run Rotate While Moving *" type="float" desc="(this # x57.29578)" optional="1" funcr="MultiplyRounded(57.29578,6)" funcw="DivideRounded(57.29578, 6)"/>
	#
	# You can handle only this value type:
	#				ElementValueType.Float
	#				ElementValueType.Int
	#				ElementValueType.Short
	#				ElementValueType.UInt
	#				ElementValueType.UShort
	#
	# The Element structure:
	#
	#     public class PyElement
	#     {
	#         public string Name;
	#         public string Desc; //Description
	#     
	#         public int CondId;
	#         public string FormIdType;
	#         public string[] Flags;
	#         public int Group;
	#         public bool Multiline;
	#         public bool NotInInfo;
	#         public bool Optional;
	#         public string[] Options;
	#         public int Repeat;
	#         public string FuncRead;
	#         public string FuncWrite;
	#         public ElementValueType ValueType;
	#     }
	
	#*********************************************************
	#*** P R O T O T Y P E S
	#*********************************************************
	
	#proto: START
	
	#RadiansNormalize(self, element, value)
	
	#ReturnZeroIfEqual(self, element, value, float)
	#ReturnZeroIfEqualRounded(self, element, value, float, int)
	
	#ReturnValueIfEqualZero(self, element, value, float)
	#ReturnValueIfEqualZeroRounded(self, element, value, float, int)	
	
	#Multiply(self, element, value, float)
    #MultiplyRounded(self, element, value, float, int)
	
	#Divide(self, element, value, float)
	#DivideRounded(self, element, value, float, int)

	#proto: END

	#*********************************************************
	#*** I N I T   C L A S S
	#*********************************************************

	def __init__(self):
		pass
		
	#*********************************************************
	#*** F U N C T I O N S
	#*********************************************************

	#***************************************************
	#*** RadiansNormalize
	#***************************************************

	def SingleSameValue(self, A, B):
			SingleResolution = 0.00000499999999999999999999
			return abs(A - B) <= max(min(abs(A), abs(B)) * SingleResolution, SingleResolution)
			
	def RadiansNormalize(self, element, value, param):
		
		Result = value	
		TwoPi = math.pi*2
		
		while Result < 0.0:
			Result = Result + TwoPi
		
		while Result > 0.0:
			Result = Result - TwoPi
			
		if self.SingleSameValue(Result, 0.0) or (Result < 0.0): 
			Result = 0.0		
			
		if self.SingleSameValue(Result, TwoPi) or (Result > TwoPi):
			Result = 0.0

		return Result

	#***************************************************
	#*** ReturnZeroIfEqual
	#***************************************************
	def ReturnZeroIfEqual(self, element, value, param):
		testValue = param[0]
		if testValue == value:
			return 0.0
		else:
			return value

	#***************************************************
	#*** ReturnZeroIfEqualRounded
	#***************************************************
	def ReturnZeroIfEqualRounded(self, element, value, param):
		testValue = param[0]
		rounded = param[1]
		value = round(value, rounded)
		if testValue == value:
			return 0.0
		else:
			return value
			
	#***************************************************
	#*** ReturnValueIfEqualZero
	#***************************************************
	def ReturnValueIfEqualZero(self, element, value, param):
		valueReturned = param[0]
		if value == 0:
			return valueReturned
		else:
			return value
			
	#***************************************************
	#*** ReturnValueIfEqualZeroRounded
	#***************************************************
	def ReturnValueIfEqualZeroRounded(self, element, value, param):
		valueReturned = param[0]
		rounded = param[1]
		value = round(value, rounded)
		if value == 0:
			return valueReturned
		else:
			return value
			
	#***************************************************
	#*** MULTIPLY
	#***************************************************
	def Multiply(self, element, value, param):
		return value * param[0]

	def MultiplyRounded(self, element, value, param):
		multiplier = param[0]
		rounded = param[1]
		return round(value * multiplier, rounded)

	#***************************************************
	#*** DIVIDE
	#***************************************************
	def Divide(self, element, value, param):
		return value / param[0]

	def DivideRounded(self, element, value, param):
		divider = param[0]
		rounded = param[1]
		return round(value / divider, rounded)
